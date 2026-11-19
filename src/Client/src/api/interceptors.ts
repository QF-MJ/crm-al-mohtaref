import { toast } from 'sonner'
import { api } from './client'
import { toApiError } from './errors'
import { queryClient } from '@/lib/queryClient'
import { useAuthStore } from '@/stores/auth.store'
import i18n from '@/i18n'

let installed = false
let sessionExpiredNotified = false

// مسارات لا تحتاج Token، و401 منها معناه "بيانات خاطئة" وليس "انتهت الجلسة"
const PUBLIC_PATHS = ['/auth/login']

const isPublic = (url?: string) => PUBLIC_PATHS.some((path) => url?.endsWith(path))

// يُستدعى مرة واحدة من main.tsx
export function setupInterceptors() {
  if (installed) return
  installed = true

  api.interceptors.request.use((config) => {
    const token = useAuthStore.getState().token
    if (token && !isPublic(config.url)) config.headers.Authorization = `Bearer ${token}`
    return config
  })

  api.interceptors.response.use(
    (response) => response,
    (error: unknown) => {
      const apiError = toApiError(error)
      const url = (error as { config?: { url?: string } })?.config?.url

      // 401 خارج صفحات الدخول = انتهت الجلسة أو أُبطل التوكن
      if (apiError.status === 401 && !isPublic(url)) {
        const { user, logout } = useAuthStore.getState()
        if (user) {
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
      }

      // نرفض دائماً بـ ApiError، فكل الواجهة تتعامل مع شكل واحد
      return Promise.reject(apiError)
    },
  )
}
