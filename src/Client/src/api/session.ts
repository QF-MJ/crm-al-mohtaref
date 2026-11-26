import { toast } from 'sonner'
import { api } from './client'
import { toApiError } from './errors'
import { queryClient } from '@/lib/queryClient'
import { AUTH_STORAGE_KEY, isTokenExpired, useAuthStore } from '@/stores/auth.store'
import type { RefreshTokenDto, RefreshTokenRequest } from '@/features/auth/types'
import i18n from '@/i18n'

// إدارة الجلسة: تجديد الـ Access Token، مزامنة التبويبات، وإنهاء الجلسة.
// الباك يبدّل الـ refresh token مع كل تجديد، وإذا استُعمل توكن ملغى مرة ثانية
// يلغي كل جلسات المستخدم. لذلك يجب ألا يجدّد تبويبان (أو طلبان) بنفس التوكن معاً.

export const AUTH_PATHS = {
  login: '/auth/login',
  refresh: '/auth/refresh',
  logout: '/auth/logout',
} as const

const AUTH_PATH_LIST = Object.values(AUTH_PATHS)

/** طلبات المصادقة نفسها: لا تحمل Bearer ولا تمر بآلية التجديد (تمنع الحلقة اللانهائية) */
export const isAuthPath = (url?: string) => AUTH_PATH_LIST.some((path) => url?.endsWith(path))

const REFRESH_LOCK_NAME = 'crm-auth-refresh'

/**
 * فشل التجديد. `sessionEnded` = الباك رفض التوكن (منتهٍ/ملغى/غير صالح) فالجلسة انتهت فعلاً.
 * أما أخطاء الشبكة و5xx فلا تنهي الجلسة: نحتفظ بالتوكنات ونحاول لاحقاً.
 */
export class RefreshFailedError extends Error {
  readonly sessionEnded: boolean

  constructor(sessionEnded: boolean, message: string) {
    super(message)
    this.name = 'RefreshFailedError'
    this.sessionEnded = sessionEnded
  }
}

// الباك يرجّع 401 للتوكن المنتهي/الملغى، 400 للشكل الخاطئ، 404 إذا حُذف المستخدم
const SESSION_ENDING_STATUSES = new Set([400, 401, 403, 404])

// وعد واحد مشترك داخل التبويب: عدة طلبات رجعت 401 معاً = طلب تجديد واحد
let inFlightRefresh: Promise<string> | null = null

/**
 * يرجّع Access Token صالحاً، ويجدّده فقط عند الحاجة.
 * @param staleToken التوكن الذي رُفض (أو انتهى). إذا كان التوكن الحالي مختلفاً عنه
 *   وما زال صالحاً، فهناك من جدّد قبلنا (طلب آخر أو تبويب آخر) ونستعمله مباشرة.
 */
export function refreshAccessToken(staleToken: string | null): Promise<string> {
  inFlightRefresh ??= withRefreshLock(() => refreshIfStale(staleToken)).finally(() => {
    inFlightRefresh = null
  })
  return inFlightRefresh
}

async function refreshIfStale(staleToken: string | null): Promise<string> {
  // ربما جدّد تبويب آخر أثناء انتظارنا للقفل: نقرأ آخر نسخة من localStorage
  await useAuthStore.persist.rehydrate()
  const { token, refreshToken, user } = useAuthStore.getState()

  if (token && token !== staleToken && !isTokenExpired(user)) return token
  if (!refreshToken) throw new RefreshFailedError(true, 'No refresh token')

  let dto: RefreshTokenDto
  try {
    const body: RefreshTokenRequest = { refreshToken }
    dto = (await api.post<RefreshTokenDto>(AUTH_PATHS.refresh, body)).data
  } catch (error) {
    const { status, message } = toApiError(error)
    throw new RefreshFailedError(SESSION_ENDING_STATUSES.has(status), message)
  }

  // خرج المستخدم (أو تغيّرت الجلسة) أثناء انتظار الرد: لا نعيد إحياء جلسة انتهت
  if (useAuthStore.getState().refreshToken !== refreshToken) {
    throw new RefreshFailedError(false, 'Session changed during refresh')
  }

  useAuthStore.getState().applyRefresh(dto)
  return dto.token
}

/**
 * Web Locks API: قفل مشترك بين كل تبويبات نفس الموقع، فلا يجدّد إلا تبويب واحد في كل لحظة.
 * المتصفحات القديمة جداً لا تدعمه، فننفّذ مباشرة (يبقى الوعد المشترك داخل التبويب).
 */
function withRefreshLock<T>(task: () => Promise<T>): Promise<T> {
  if (typeof navigator === 'undefined' || !navigator.locks) return task()
  return navigator.locks.request(REFRESH_LOCK_NAME, task)
}

let sessionExpiredNotified = false

/** ينهي الجلسة محلياً: يمسح التوكنات والكاش ويعرض تنبيهاً واحداً. RequireAuth يحوّل لصفحة الدخول. */
export function endSession() {
  const { user, logout } = useAuthStore.getState()
  if (!user) return

  logout()
  queryClient.clear()

  if (!sessionExpiredNotified) {
    sessionExpiredNotified = true
    toast.error(i18n.t('errors.sessionExpired'))
    // نسمح بإظهار التنبيه مرة أخرى بعد فترة قصيرة
    window.setTimeout(() => {
      sessionExpiredNotified = false
    }, 5000)
  }
}

/**
 * مزامنة التبويبات: حدث storage يصل للتبويبات الأخرى فقط عند تغيّر localStorage،
 * فنعيد قراءة الجلسة. هكذا يرى كل تبويب التوكنات الجديدة، ويخرج إذا خرج المستخدم من تبويب آخر.
 */
export function setupSessionSync() {
  window.addEventListener('storage', (event) => {
    if (event.key !== AUTH_STORAGE_KEY) return
    const hadUser = useAuthStore.getState().user !== null
    void Promise.resolve(useAuthStore.persist.rehydrate()).then(() => {
      if (hadUser && !useAuthStore.getState().user) queryClient.clear()
    })
  })
}
