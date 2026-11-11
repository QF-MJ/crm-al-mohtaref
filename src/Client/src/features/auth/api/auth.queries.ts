import { useMutation } from '@tanstack/react-query'
import { useAuthStore } from '@/stores/auth.store'
import type { ApiError } from '@/types/common'
import type { LoginForm } from '../schemas/login.schema'
import type { LoginResponse } from '../types'
import { loginRequest } from './auth.api'

export function useLogin() {
  const login = useAuthStore((state) => state.login)
  return useMutation<LoginResponse, ApiError, LoginForm>({
    mutationFn: loginRequest,
    onSuccess: (response, request) => login(response, request.accountName),
  })
}

export function useLogout() {
  const clearSession = useAuthStore((state) => state.logout)

  return useMutation({
    mutationFn: async () => clearSession(),
  })
}
