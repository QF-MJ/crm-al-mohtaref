import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { getApiErrorMessage } from '@/api/errors'
import { useLogin } from '../api/auth.queries'
import { loginSchema, type LoginForm } from '../schemas/login.schema'

export function LoginPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const location = useLocation()
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
      },
      onError: (error) => toast.error(getApiErrorMessage(error, t('loginError'))),
    })

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
}
