<<<<<<< Updated upstream
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { getApiErrorMessage } from '@/api/errors'
=======
import { useTranslation } from 'react-i18next'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { paths } from '@/app/router/paths'
import { useAuthStore } from '@/stores/auth.store'
>>>>>>> Stashed changes
import { useLogin } from '../api/auth.queries'
import { LoginForm } from '../components/LoginForm'
import type { LoginForm as LoginFormValues } from '../schemas/login.schema'

<<<<<<< Updated upstream
=======
interface LocationState {
  from?: { pathname: string; search?: string }
}

const LOCKED_STATUSES = [403, 423]
const RATE_LIMITED_STATUSES = [422, 429]

>>>>>>> Stashed changes
export function LoginPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const location = useLocation()
<<<<<<< Updated upstream
  const { mutate, isPending } = useLogin()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginForm>({ resolver: zodResolver(loginSchema) })

  const onSubmit = (values: LoginForm) =>
    mutate(values, {
      onSuccess: () => {
        const from = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname
        navigate(from ?? '/', { replace: true })
=======
  const isLoggedIn = useAuthStore((state) => state.user !== null)
  const { mutate, isPending } = useLogin()

  const from = (location.state as LocationState | null)?.from
  const target = from ? `${from.pathname}${from.search ?? ''}` : paths.home

  if (isLoggedIn) return <Navigate to={target} replace />

  const onSubmit = (values: LoginFormValues) => {
    mutate(values, {
      onSuccess: () => navigate(target, { replace: true }),
      onError: (error) => {
        const message =
          error.status === 0
            ? t('network')
            : RATE_LIMITED_STATUSES.includes(error.status)
              ? t('tooManyAttempts')
              : LOCKED_STATUSES.includes(error.status)
                ? error.message || t('locked')
                : error.message || t('failed')

        toast.error(message)
>>>>>>> Stashed changes
      },
      onError: (error) => toast.error(getApiErrorMessage(error, t('loginError'))),
    })
<<<<<<< Updated upstream

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="w-full max-w-sm space-y-4 rounded-lg border bg-card p-6"
    >
      <h1 className="text-xl font-bold">{t('title')}</h1>

      <div className="space-y-2">
        <Label htmlFor="accountName">{t('username')}</Label>
        {/* dir="auto": الاسم قد يكون عربياً أو إنجليزياً */}
        <Input id="accountName" dir="auto" autoComplete="username" {...register('accountName')} />
        {errors.accountName && <p className="text-sm text-destructive">{t('required')}</p>}
      </div>

      <div className="space-y-2">
        <Label htmlFor="password">{t('password')}</Label>
        <Input
          id="password"
          type="password"
          dir="ltr"
          autoComplete="current-password"
          {...register('password')}
        />
        {errors.password && <p className="text-sm text-destructive">{t('required')}</p>}
      </div>

      <Button type="submit" className="w-full" disabled={isPending}>
        {t('submit')}
      </Button>
    </form>
  )
=======
  }

  return <LoginForm onSubmit={onSubmit} isPending={isPending} />
>>>>>>> Stashed changes
}
