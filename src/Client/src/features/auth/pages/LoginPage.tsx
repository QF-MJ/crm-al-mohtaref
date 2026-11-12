import { useTranslation } from 'react-i18next'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { paths } from '@/app/router/paths'
import { useAuthStore } from '@/stores/auth.store'
import { useLogin } from '../api/auth.queries'
import { LoginForm } from '../components/LoginForm'
import type { LoginForm as LoginFormValues } from '../schemas/login.schema'

interface LocationState {
  from?: { pathname: string; search?: string }
}

const LOCKED_STATUSES = [403, 423]
const RATE_LIMITED_STATUSES = [422, 429]
const NETWORK_ERROR_STATUS = 0

export function LoginPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const location = useLocation()
  const isLoggedIn = useAuthStore((state) => state.user !== null)
  const { mutate, isPending } = useLogin()

  const from = (location.state as LocationState | null)?.from
  const target = from ? `${from.pathname}${from.search ?? ''}` : paths.home

  if (isLoggedIn) return <Navigate to={target} replace />

  const resolveErrorText = (status: number, message: string) => {
    if (status === NETWORK_ERROR_STATUS) return t('network')
    if (RATE_LIMITED_STATUSES.includes(status)) return t('tooManyAttempts')
    if (LOCKED_STATUSES.includes(status)) return message || t('locked')
    return message || t('failed')
  }

  const onSubmit = (values: LoginFormValues) =>
    mutate(values, {
      onSuccess: () => navigate(target, { replace: true }),
      onError: (error) => {
        const { status, message } = error
        toast.error(resolveErrorText(status, message))
      },
    })

  return <LoginForm onSubmit={onSubmit} isPending={isPending} />
}
