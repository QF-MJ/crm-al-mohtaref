import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { paths } from '@/app/router/paths'
import { useAuthStore } from '@/stores/auth.store'
import { toApiError } from '@/api/errors'
import { useLogin } from '../api/auth.queries'
import { LoginForm } from '../components/LoginForm'
import type { LoginForm as LoginFormValues } from '../schemas/login.schema'

// RequireAuth يمرّر الصفحة المطلوبة بالـ state ليعود إليها المستخدم بعد الدخول
interface LocationState {
  from?: { pathname?: string; search?: string }
}

// الباك يرجّع 403 للحساب المقفل (الـ OpenAPI يقول 423)، و422 عند تجاوز حد المحاولات
const LOCKED_STATUSES = [403, 423]
const RATE_LIMITED_STATUSES = [422, 429]

// الأخطاء تُعرض داخل النموذج، لا كـ toast يختفي
export function LoginPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const location = useLocation()
  const isLoggedIn = useAuthStore((state) => state.user !== null)
  const { mutate, isPending } = useLogin()
  const [errorMessage, setErrorMessage] = useState('')

  const from = (location.state as LocationState | null)?.from
  // نقبل مسارات داخلية فقط، لا روابط خارجية
  const target =
    from?.pathname && from.pathname.startsWith('/')
      ? `${from.pathname}${from.search ?? ''}`
      : paths.dashboard

  if (isLoggedIn) return <Navigate to={target} replace />

  const resolveError = (status: number, message: string) => {
    if (status === 0) return t('network')
    if (RATE_LIMITED_STATUSES.includes(status)) return t('tooManyAttempts')
    if (LOCKED_STATUSES.includes(status)) return message || t('locked')
    return message || t('failed')
  }

  const onSubmit = (values: LoginFormValues) => {
    setErrorMessage('')
    mutate(values, {
      onSuccess: () => navigate(target, { replace: true }),
      onError: (error) => {
        const { status, message } = toApiError(error)
        setErrorMessage(resolveError(status, message))
      },
    })
  }

  return (
    <LoginForm onSubmit={onSubmit} isPending={isPending} errorMessage={errorMessage || undefined} />
  )
}
