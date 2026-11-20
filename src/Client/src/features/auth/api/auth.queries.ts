import { useMutation } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { useAuthStore } from '@/stores/auth.store'
import { loginRequest, logoutRequest } from './auth.api'

export function useLogin() {
  const login = useAuthStore((s) => s.login)
  return useMutation({
    mutationFn: loginRequest,
    onSuccess: (data) => login(data.user, data.token),
  })
}

export function useLogout() {
  const logout = useAuthStore((s) => s.logout)
  const { t } = useTranslation()

  return useMutation({
    mutationFn: logoutRequest,
    // في حال تعذّر الاتصال بالخادم نُنهي الجلسة محلياً حسب سيناريو تسجيل الخروج
    onSettled: () => logout(),
    onSuccess: () => {
      toast.success(t('logout.success'))
    },
    onError: () => {
      toast.success(t('logout.success'))
    },
  })
}
