import axios from 'axios'
import { toast } from 'sonner'
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
    const isLoginRequest = /\/auth\/login(?:\?|$)/.test(config.url ?? '')
    if (isLoginRequest) config.headers.delete('Authorization')
    else if (token) config.headers.Authorization = `Bearer ${token}`
    return config
  })

  api.interceptors.response.use(
    (response) => response,
    (error: unknown) => {
      const apiError = toApiError(error)
      const isLoginRequest =
        axios.isAxiosError(error) && /\/auth\/login(?:\?|$)/.test(error.config?.url ?? '')

      if (apiError.status === 401 && !isLoginRequest) {
        useAuthStore.getState().logout()
        toast.error(apiError.message)
      }

      return Promise.reject(apiError)
    },
  )
}
