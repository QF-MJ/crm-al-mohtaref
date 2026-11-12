import { useMutation } from '@tanstack/react-query'
import { useAuthStore } from '@/stores/auth.store'
import type { ApiError } from '@/types/common'
import { loginRequest } from './auth.api'
import type { LoginForm } from '../schemas/login.schema'
import type { LoginDto } from '../types'

export function useLogin() {
  const login = useAuthStore((state) => state.login)
  return useMutation<LoginDto, ApiError, LoginForm>({
    mutationFn: loginRequest,
    onSuccess: (response) => login(response),
  })
}

export function useLogout() {
  const logout = useAuthStore((state) => state.logout)
  return useMutation({
    mutationFn: async () => logout(),
  })
}
