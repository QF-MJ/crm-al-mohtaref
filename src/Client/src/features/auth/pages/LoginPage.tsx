import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { Eye, EyeOff } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { paths } from '@/app/router/paths'
import { useAuthStore } from '@/stores/auth.store'
import type { ApiError } from '@/types/common'
import { useLogin } from '../api/auth.queries'
import { LoginForm } from '../components/LoginForm'
import type { LoginForm as LoginFormValues } from '../schemas/login.schema'

// RequireAuth Ì„—¯— «·’›Õ… «·„ÿ·Ê»… »«·‹ state ·Ì—Ã⁄ ≈·ÌÂ« «·„” Œœ„ »⁄œ «·œŒÊ·
interface LocationState {
  from?: { pathname: string; search?: string }
}

// «·»«ﬂ Ì—Ã¯⁄ 403 ··Õ”«» «·„ﬁ›Ê· («·‹ OpenAPI ÌﬁÊ· 423)° ÊÌ—Ã¯⁄ 422 »œÊ‰ body ⁄‰œ  Ã«Ê“ Õœ «·„Õ«Ê·« 
const LOCKED_STATUSES = [403, 423]
const RATE_LIMITED_STATUSES = [422, 429]
const NETWORK_ERROR_STATUS = 0

export function LoginPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const location = useLocation()
  const isLoggedIn = useAuthStore((s) => s.user !== null)
  const { mutate, isPending } = useLogin()
  const [showPassword, setShowPassword] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginForm>({ resolver: zodResolver(loginSchema) })

  const from = (location.state as LocationState | null)?.from
  const target = from ? `${from.pathname}${from.search ?? ''}` : paths.home

  // „” Œœ„ „”Ã¯· √’·« ·« ÌÕ «Ã ’›Õ… «·œŒÊ·
  if (isLoggedIn) return <Navigate to={target} replace />

  const onSubmit = (values: LoginForm) =>
    mutate(values, {
      onSuccess: () => navigate(target, { replace: true }),
      onError: (error) => {
        const { status, message } = error as unknown as ApiError
        toast.error(resolveErrorText(status, message))
      },
    })

  // —”«·… «·”Ì—›— (⁄—»Ì… Ê⁄«„…° ·«  ﬂ‘› ≈‰ ﬂ«‰ «·Õ”«» „ÊÃÊœ«) ≈·« »Õ«·«  ·« Ì—”· ›ÌÂ« —”«·… „›Ìœ…
  const resolveErrorText = (status: number, message: string) => {
    if (status === NETWORK_ERROR_STATUS) return t('network')
    if (RATE_LIMITED_STATUSES.includes(status)) return t('tooManyAttempts')
    if (LOCKED_STATUSES.includes(status)) return message || t('locked')
    return message || t('failed')
  }

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      noValidate
      className="w-full max-w-md space-y-6 rounded-lg border bg-card p-8"
    >
      <header className="space-y-4 text-center">
        <img src="/vertical%20logo.svg" alt={t('logoAlt')} className="mx-auto h-20 dark:hidden" />
        <img
          src="/vertical%20logo%20white.svg"
          alt={t('logoAlt')}
          className="mx-auto hidden h-20 dark:block"
        />
        <p className="text-sm text-muted-foreground">{t('systemName')}</p>
      </header>

      <div className="space-y-1">
        <h1 className="text-2xl font-bold">{t('title')}</h1>
        <p className="text-sm text-muted-foreground">{t('subtitle')}</p>
      </div>

      <div className="space-y-2">
        <Label htmlFor="accountName">{t('accountName')}</Label>
        {/* dir="auto": «”„ «·Õ”«» ﬁœ ÌﬂÊ‰ ⁄—»Ì« √Ê ≈‰Ã·Ì“Ì« */}
        <Input
          id="accountName"
          dir="auto"
          autoComplete="username"
          placeholder={t('accountNamePlaceholder')}
          aria-invalid={!!errors.accountName}
          {...register('accountName')}
        />
        {errors.accountName && <p className="text-sm text-destructive">{t('required')}</p>}
      </div>

      <div className="space-y-2">
        <Label htmlFor="password">{t('password')}</Label>
        <div className="relative">
          <Input
            id="password"
            type={showPassword ? 'text' : 'password'}
            dir="ltr"
            autoComplete="current-password"
            placeholder={t('passwordPlaceholder')}
            aria-invalid={!!errors.password}
            className="ps-10"
            {...register('password')}
          />
          <button
            type="button"
            onClick={() => setShowPassword((v) => !v)}
            aria-label={showPassword ? t('hidePassword') : t('showPassword')}
            className="absolute inset-y-0 end-3 flex items-center text-muted-foreground hover:text-foreground"
          >
            {showPassword ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
          </button>
        </div>
        {errors.password && <p className="text-sm text-destructive">{t('required')}</p>}

        <label className="flex w-fit cursor-pointer items-center gap-2 text-xs text-muted-foreground">
          <input
            type="checkbox"
            checked={showPassword}
            onChange={(e) => setShowPassword(e.target.checked)}
          />
          {t('showPassword')}
        </label>
      </div>

  return <LoginForm onSubmit={onSubmit} isPending={isPending} />
}
