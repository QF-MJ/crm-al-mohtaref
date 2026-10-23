import { api } from './client'
import { toApiError } from './errors'
import { useAuthStore } from '@/stores/auth.store'

let installed = false

// يُستدعى مرة واحدة من main.tsx
export function setupInterceptors() {
  if (installed) return
  installed = true

  api.interceptors.request.use((config) => {
    const token = useAuthStore.getState().token
    if (token) config.headers.Authorization = `Bearer ${token}`
    return config
  })

  api.interceptors.response.use(
    (response) => response,
    (error: unknown) => {
      const apiError = toApiError(error)
      if (apiError.status === 401) useAuthStore.getState().logout()
      // نرفض دائماً بـ ApiError، فكل الواجهة تتعامل مع شكل واحد
      return Promise.reject(apiError)
    },
  )
}
