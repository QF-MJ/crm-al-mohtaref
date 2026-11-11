import { useMutation } from '@tanstack/react-query'
import { useAuthStore } from '@/stores/auth.store'
import { loginRequest, logoutRequest } from './auth.api'
import type { LoginForm } from '../schemas/login.schema'

export function useLogin() {
  const login = useAuthStore((s) => s.login)
  return useMutation({
    // نبني طلب الباك من حقول الفورم هنا فقط
    mutationFn: (form: LoginForm) =>
      loginRequest({ accountName: form.accountName, password: form.password }),
    onSuccess: (dto) => login(dto),
  })
}

export function useLogout() {
  const logout = useAuthStore((s) => s.logout)
  return useMutation({
    mutationFn: async () => {
      const refreshToken = useAuthStore.getState().refreshToken
      // نحاول إلغاء الـ refresh token بالسيرفر، لكن الخروج المحلي يحصل دائماً
      if (refreshToken) await logoutRequest({ refreshToken })
    },
    onSettled: () => logout(),
  })
}
