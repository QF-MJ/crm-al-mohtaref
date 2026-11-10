import { useMutation } from '@tanstack/react-query'
import { useAuthStore } from '@/stores/auth.store'
import { loginRequest } from './auth.api'

export function useLogin() {
  const login = useAuthStore((s) => s.login)
  return useMutation({
    mutationFn: loginRequest,
    onSuccess: login,
  })
}

export function useLogout() {
  const clearSession = useAuthStore((state) => state.logout)

  return useMutation({
    mutationFn: async () => clearSession(),
  })
}
