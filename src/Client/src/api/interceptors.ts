import axios from 'axios'
import { api } from './client'
import { toApiError } from './errors'
import { useAuthStore } from '@/stores/auth.store'

let installed = false

// مسارات لا تحتاج Token، و401 منها معناه "بيانات خاطئة" وليس "انتهت الجلسة"
const PUBLIC_PATHS = ['/auth/login']

const isPublic = (url?: string) => PUBLIC_PATHS.some((p) => url?.endsWith(p))

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
      // 401 من login لا يعني انتهاء الجلسة، فلا نمسح شيئاً
      if (apiError.status === 401 && !isPublic(url)) useAuthStore.getState().logout()
      // نرفض دائماً بـ ApiError، فكل الواجهة تتعامل مع شكل واحد
      return Promise.reject(apiError)
    },
  )
}
