import { useTranslation } from 'react-i18next'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { paths } from '@/app/router/paths'
import { getApiErrorMessage } from '@/api/errors'
import { useAuthStore } from '@/stores/auth.store'
import { useLogin } from '../api/auth.queries'
import { LoginForm } from '../components/LoginForm'
import type { LoginForm as LoginFormValues } from '../schemas/login.schema'

interface LocationState {
  from?: { pathname: string; search?: string }
}

export function LoginPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const location = useLocation()
  const isLoggedIn = useAuthStore((state) => state.user !== null)
  const { mutate, isPending } = useLogin()

  const from = (location.state as LocationState | null)?.from
  const target = from ? `${from.pathname}${from.search ?? ''}` : paths.home

  if (isLoggedIn) return <Navigate to={target} replace />

  const onSubmit = (values: LoginFormValues) => {
    mutate(values, {
      onSuccess: () => navigate(target, { replace: true }),
      onError: (error) => toast.error(getApiErrorMessage(error, t('loginError'))),
    })
  }

  return <LoginForm onSubmit={onSubmit} isPending={isPending} />
}
