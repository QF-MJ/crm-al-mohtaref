import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { AlertCircle, Eye, EyeOff, KeyRound, LoaderCircle, UserRound } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { loginSchema, type LoginForm as LoginFormValues } from '../schemas/login.schema'

interface LoginFormProps {
  onSubmit: (values: LoginFormValues) => void
  isPending: boolean
  errorMessage?: string
}

export function LoginForm({ onSubmit, isPending, errorMessage }: LoginFormProps) {
  const { t } = useTranslation('auth')
  const [showPassword, setShowPassword] = useState(false)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { accountName: '', password: '' },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="w-full max-w-md">
      <header className="mb-8 space-y-3">
        <p className="text-sm font-semibold text-primary">{t('formEyebrow')}</p>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
        <p className="text-sm leading-7 text-slate-500">{t('subtitle')}</p>
      </header>

      {errorMessage && (
        <div
          role="alert"
          aria-live="polite"
          className="mb-5 flex items-start gap-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800"
        >
          <AlertCircle aria-hidden="true" className="mt-0.5 size-[18px] shrink-0" />
          <p className="leading-6">{errorMessage}</p>
        </div>
      )}

      <div className="mb-5 space-y-2">
        <Label htmlFor="accountName" className="text-slate-700">
          {t('accountName')}
        </Label>
        <div className="relative">
          <UserRound
            aria-hidden="true"
            className="pointer-events-none absolute start-3.5 top-1/2 size-[18px] -translate-y-1/2 text-slate-400"
          />
          <Input
            id="accountName"
            dir="auto"
            autoComplete="username"
            autoCapitalize="none"
            autoCorrect="off"
            spellCheck={false}
            placeholder={t('accountNamePlaceholder')}
            disabled={isPending}
            aria-invalid={Boolean(errors.accountName)}
            aria-describedby={errors.accountName ? 'accountName-error' : undefined}
            className="h-12 rounded-xl border-slate-200 bg-slate-50/70 ps-11 pe-4 placeholder:text-slate-400 focus-visible:bg-white"
            {...register('accountName')}
          />
        </div>
        {errors.accountName && (
          <p id="accountName-error" role="alert" className="text-sm text-destructive">
            {t('required')}
          </p>
        )}
      </div>

      <div className="mb-7 space-y-2">
        <Label htmlFor="password" className="text-slate-700">
          {t('password')}
        </Label>
        <div className="relative">
          <KeyRound
            aria-hidden="true"
            className="pointer-events-none absolute start-3.5 top-1/2 size-[18px] -translate-y-1/2 text-slate-400"
          />
          <Input
            id="password"
            type={showPassword ? 'text' : 'password'}
            dir="ltr"
            autoComplete="current-password"
            placeholder={t('passwordPlaceholder')}
            disabled={isPending}
            aria-invalid={Boolean(errors.password)}
            aria-describedby={errors.password ? 'password-error' : undefined}
            className="h-12 rounded-xl border-slate-200 bg-slate-50/70 ps-11 pe-12 placeholder:text-slate-400 focus-visible:bg-white"
            {...register('password')}
          />
          <button
            type="button"
            onClick={() => setShowPassword((visible) => !visible)}
            aria-label={showPassword ? t('hidePassword') : t('showPassword')}
            aria-pressed={showPassword}
            disabled={isPending}
            className="absolute inset-y-0 end-3 flex items-center text-slate-400 transition-colors hover:text-slate-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/50"
          >
            {showPassword ? (
              <EyeOff aria-hidden="true" className="size-[18px]" />
            ) : (
              <Eye aria-hidden="true" className="size-[18px]" />
            )}
          </button>
        </div>
        {errors.password && (
          <p id="password-error" role="alert" className="text-sm text-destructive">
            {t('required')}
          </p>
        )}
      </div>

      <Button
        type="submit"
        size="lg"
        disabled={isPending}
        className="h-12 w-full rounded-xl bg-primary text-base shadow-lg shadow-teal-900/10 hover:bg-primary/90"
      >
        {isPending && <LoaderCircle aria-hidden="true" className="animate-spin" />}
        {t(isPending ? 'submitting' : 'submit')}
      </Button>

      <p className="mt-5 text-center text-xs leading-6 text-slate-400">{t('formFooter')}</p>
    </form>
  )
}
