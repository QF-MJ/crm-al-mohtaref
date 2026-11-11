import { useMutation } from '@tanstack/react-query'
import { useAuthStore } from '@/stores/auth.store'
<<<<<<< Updated upstream
import { loginRequest } from './auth.api'

export function useLogin() {
  const login = useAuthStore((s) => s.login)
  return useMutation({
    mutationFn: loginRequest,
    onSuccess: login,
=======
import type { ApiError } from '@/types/common'
import { loginRequest, logoutRequest } from './auth.api'
import type { LoginDto } from '../types'
import type { LoginForm } from '../schemas/login.schema'

export function useLogin() {
  const login = useAuthStore((s) => s.login)
  return useMutation<LoginDto, ApiError, LoginForm>({
    // نبني طلب الباك من حقول الفورم هنا فقط
    mutationFn: (form: LoginForm) =>
      loginRequest({ accountName: form.accountName, password: form.password }),
    onSuccess: (dto, form) => login(dto, form.accountName),
>>>>>>> Stashed changes
  })
}

export function useLogout() {
  const clearSession = useAuthStore((state) => state.logout)

  return useMutation({
    mutationFn: async () => clearSession(),
  })
}
