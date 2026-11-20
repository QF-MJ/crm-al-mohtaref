import { useMutation } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { useAuthStore } from '@/stores/auth.store'
import { loginRequest, logoutRequest } from './auth.api'

export function useLogin() {
  const login = useAuthStore((state) => state.login)
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: LoginForm) => loginRequest(values),
    // نمسح كاش المستخدم السابق قبل تثبيت الجلسة الجديدة، حتى لا تظهر بياناته لصاحب صلاحية أخرى
    onSuccess: (dto) => {
      queryClient.clear()
      login(dto)
    },
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
