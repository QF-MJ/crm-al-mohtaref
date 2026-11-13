import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { paths } from '@/app/router/paths'
import { useAuthStore } from '@/stores/auth.store'
import { describeApiError, isHumanMessage, toApiError } from '@/api/errors'
import { useLogin } from '../api/auth.queries'
import { LoginForm } from '../components/LoginForm'
import type { LoginForm as LoginFormValues } from '../schemas/login.schema'

// RequireAuth يمرّر الصفحة المطلوبة بالـ state ليعود إليها المستخدم بعد الدخول
interface LocationState {
  from?: { pathname?: string; search?: string }
}

// الأخطاء تُعرض داخل النموذج، لا كـ toast يختفي
export function LoginPage() {
  const { t } = useTranslation()
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

  const resolveError = (error: unknown) => {
    const { status, message } = toApiError(error)
    // 403 في تسجيل الدخول = الحساب مقفول (الباك يرسل رسالة عربية عادة)
    if (status === 403) {
      return isHumanMessage(message) ? message.trim() : t('errors.accountLocked')
    }
    // 422/429 = تجاوز حد محاولات الدخول
    if (status === 422 || status === 429) return t('errors.tooManyAttempts')
    return describeApiError(error, t)
  }

  const onSubmit = (values: LoginFormValues) => {
    setErrorMessage('')
    mutate(values, {
      onSuccess: () => navigate(target, { replace: true }),
      onError: (error) => setErrorMessage(resolveError(error)),
    })
  }

  return (
    <LoginForm onSubmit={onSubmit} isPending={isPending} errorMessage={errorMessage || undefined} />
  )
}
