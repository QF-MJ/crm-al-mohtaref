import axios, { type InternalAxiosRequestConfig } from 'axios'
import { api } from './client'
import { toApiError } from './errors'
import {
  endSession,
  isAuthPath,
  refreshAccessToken,
  RefreshFailedError,
  setupSessionSync,
} from './session'
import { useAuthStore } from '@/stores/auth.store'

let installed = false

// علامة على الطلب نفسه: أُعيد مرة بعد التجديد، فلا نجدّد له مرة ثانية
interface RetriableConfig extends InternalAxiosRequestConfig {
  _retriedAfterRefresh?: boolean
}

const BEARER_PREFIX = 'Bearer '

function bearerTokenOf(config: InternalAxiosRequestConfig): string | null {
  const header = config.headers?.Authorization
  return typeof header === 'string' && header.startsWith(BEARER_PREFIX)
    ? header.slice(BEARER_PREFIX.length)
    : null
}

// يُستدعى مرة واحدة من main.tsx
export function setupInterceptors() {
  if (installed) return
  installed = true

  setupSessionSync()

  api.interceptors.request.use((config) => {
    const token = useAuthStore.getState().token
    if (token && !isAuthPath(config.url)) config.headers.Authorization = `${BEARER_PREFIX}${token}`
    return config
  })

  api.interceptors.response.use(
    (response) => response,
    async (error: unknown) => {
      const apiError = toApiError(error)
      const config = axios.isAxiosError(error)
        ? (error.config as RetriableConfig | undefined)
        : undefined

      // 401 من طلبات المصادقة معناه بيانات/توكن خاطئ وليس "جدّد"، فيُعالج في مكانه
      const shouldTryRefresh =
        apiError.status === 401 && config !== undefined && !isAuthPath(config.url)

      if (!shouldTryRefresh) return Promise.reject(apiError)

      // رُفض حتى بعد التجديد، أو لا يوجد مستخدم مسجّل: الجلسة غير صالحة
      if (config._retriedAfterRefresh || !useAuthStore.getState().user) {
        endSession()
        return Promise.reject(apiError)
      }

      try {
        await refreshAccessToken(bearerTokenOf(config))
      } catch (refreshError) {
        if (refreshError instanceof RefreshFailedError && refreshError.sessionEnded) endSession()
        // خطأ شبكة أثناء التجديد: نُبقي الجلسة، ويرى المستخدم خطأ الطلب الأصلي
        return Promise.reject(apiError)
      }

      // نعيد الطلب الأصلي مرة واحدة؛ interceptor الطلب يضع التوكن الجديد
      config._retriedAfterRefresh = true
      return api.request(config)
    },
  )
}
