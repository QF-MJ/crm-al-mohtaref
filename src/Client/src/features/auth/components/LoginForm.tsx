import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { AlertCircle, Eye, EyeOff, LoaderCircle } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { cn } from '@/lib/utils'
import { loginSchema, type LoginForm as LoginFormValues } from '../schemas/login.schema'

interface LoginFormProps {
  onSubmit: (values: LoginFormValues) => void
  isPending: boolean
  errorMessage?: string
}

const FIELD_CLASS =
  'mt-1.5 h-11 w-full rounded-xl border border-border bg-muted/60 px-3.5 text-sm text-foreground outline-none transition-colors placeholder:text-muted-foreground/70 focus:border-primary focus:bg-card focus:ring-4 focus:ring-primary/10 disabled:cursor-not-allowed disabled:opacity-60'

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
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="w-full">
      <header className="space-y-1.5">
        <p className="inline-flex items-center gap-2 text-xs font-medium text-muted-foreground">
          <span aria-hidden="true" className="size-1.5 rounded-full bg-primary" />
          {t('formEyebrow')}
        </p>
        <h1 className="text-[1.65rem] font-bold tracking-tight text-foreground">{t('title')}</h1>
        <p className="text-sm leading-6 text-muted-foreground">{t('subtitle')}</p>
      </header>

      {errorMessage && (
        <div
          role="alert"
          aria-live="polite"
          className="mt-5 flex items-start gap-2.5 rounded-xl border border-destructive/20 bg-destructive/5 px-3.5 py-3 text-sm text-destructive"
        >
          <AlertCircle aria-hidden="true" className="mt-0.5 size-4 shrink-0" />
          <p className="leading-6">{errorMessage}</p>
        </div>
      )}

      <div className="mt-6 space-y-4">
        <div>
          <label htmlFor="accountName" className="text-sm font-medium text-foreground">
            {t('accountName')}
          </label>
          <input
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
            className={cn(FIELD_CLASS, errors.accountName && 'border-destructive/60')}
            {...register('accountName')}
          />
          {errors.accountName && (
            <p id="accountName-error" role="alert" className="mt-1.5 text-xs text-destructive">
              {t('required')}
            </p>
          )}
        </div>

        <div>
          <label htmlFor="password" className="text-sm font-medium text-foreground">
            {t('password')}
          </label>
          <div className="relative">
            <input
              id="password"
              type={showPassword ? 'text' : 'password'}
              dir="ltr"
              autoComplete="current-password"
              placeholder={t('passwordPlaceholder')}
              disabled={isPending}
              aria-invalid={Boolean(errors.password)}
              aria-describedby={errors.password ? 'password-error' : undefined}
              className={cn(FIELD_CLASS, 'pr-11', errors.password && 'border-destructive/60')}
              {...register('password')}
            />
            <button
              type="button"
              onClick={() => setShowPassword((visible) => !visible)}
              aria-label={showPassword ? t('hidePassword') : t('showPassword')}
              aria-pressed={showPassword}
              disabled={isPending}
              className="absolute right-1.5 top-1/2 grid size-8 -translate-y-1/2 place-items-center rounded-lg text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/40 disabled:opacity-50"
            >
              {showPassword ? (
                <EyeOff aria-hidden="true" className="size-[18px]" />
              ) : (
                <Eye aria-hidden="true" className="size-[18px]" />
              )}
            </button>
          </div>
          {errors.password && (
            <p id="password-error" role="alert" className="mt-1.5 text-xs text-destructive">
              {t('required')}
            </p>
          )}
        </div>
      </div>

      <button
        type="submit"
        disabled={isPending}
        className="mt-6 flex h-11 w-full items-center justify-center gap-2 rounded-xl bg-primary text-sm font-semibold text-primary-foreground shadow-sm transition-colors hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-card disabled:cursor-not-allowed disabled:opacity-60"
      >
        {isPending && <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />}
        {t(isPending ? 'submitting' : 'submit')}
      </button>

      <p className="mt-4 text-center text-[11px] leading-6 text-muted-foreground">
        {t('formFooter')}
      </p>
    </form>
  )
}
