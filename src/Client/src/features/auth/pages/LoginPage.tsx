import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import type { ApiError } from '@/types/common'
import { useLogin } from '../api/auth.queries'
import { loginSchema, type LoginForm } from '../schemas/login.schema'

export function LoginPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const { mutate, isPending } = useLogin()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginForm>({ resolver: zodResolver(loginSchema) })

  const onSubmit = (values: LoginForm) =>
    mutate(values, {
      onSuccess: () => navigate('/', { replace: true }),
      onError: (error) => toast.error((error as unknown as ApiError).message),
    })

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="w-full max-w-sm space-y-4 rounded-lg border bg-card p-6"
    >
      <h1 className="text-xl font-bold">{t('title')}</h1>

      <div className="space-y-2">
        <Label htmlFor="username">{t('username')}</Label>
        {/* dir="auto": الاسم قد يكون عربياً أو إنجليزياً */}
        <Input id="username" dir="auto" autoComplete="username" {...register('username')} />
        {errors.username && <p className="text-sm text-destructive">{t('required')}</p>}
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
