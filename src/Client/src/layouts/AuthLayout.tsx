import { Navigate, Outlet } from 'react-router-dom'
import { Languages, ShieldCheck } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import logo from '@/assets/brand/logo.png'
import { ThemeToggle } from '@/components/common/ThemeToggle'
import { useAuthStore } from '@/stores/auth.store'
import '@/styles/login.css'

// شاشة دخول Clean Minimalist: بطاقة واحدة هادئة على خلفية ناعمة،
// مع ترويسة بسيطة (شعار + تبديل اللغة/الثيم) وتذييل صغير.
export function AuthLayout() {
  const { t, i18n } = useTranslation()
  const { t: ta } = useTranslation('auth')
  const user = useAuthStore((state) => state.user)
  if (user) return <Navigate to="/" replace />

  const nextLanguage = i18n.language.startsWith('ar') ? 'en' : 'ar'

  return (
    <main className="relative flex min-h-[100svh] flex-col overflow-hidden bg-[#f5f7f8]">
      <div aria-hidden="true" className="login-canvas pointer-events-none absolute inset-0" />

      <header className="relative z-10 flex items-center justify-between gap-3 px-5 py-4 sm:px-8 sm:py-5">
        <div className="flex min-w-0 items-center gap-3">
          <img src={logo} alt={ta('logoAlt')} className="app-logo h-8 w-auto" />
          <span aria-hidden="true" className="hidden h-6 w-px bg-border sm:block" />
          <span className="hidden truncate text-xs font-medium text-muted-foreground sm:block">
            {ta('employeePortal')}
          </span>
        </div>

        <div className="flex items-center gap-1">
          <button
            type="button"
            onClick={() => void i18n.changeLanguage(nextLanguage)}
            className="inline-flex h-9 items-center gap-1.5 rounded-lg px-2.5 text-xs font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/40"
            aria-label={t('actions.switchLanguage')}
          >
            <Languages aria-hidden="true" className="size-4" />
            <span>{t(nextLanguage === 'ar' ? 'actions.arabic' : 'actions.english')}</span>
          </button>
          <ThemeToggle />
        </div>
      </header>

      <div className="relative z-10 flex flex-1 items-center justify-center px-4 pb-6 pt-2 sm:pb-10">
        <div className="w-full max-w-[420px]">
          <div className="relative overflow-hidden rounded-3xl border border-border bg-card p-6 shadow-[0_20px_55px_-38px_rgba(11,60,55,0.45)] sm:p-8">
            <span
              aria-hidden="true"
              className="pointer-events-none absolute inset-x-12 top-0 h-px bg-gradient-to-r from-transparent via-primary/45 to-transparent"
            />
            <Outlet />
          </div>
        </div>
      </div>

      <footer className="relative z-10 flex flex-wrap items-center justify-center gap-x-2 gap-y-1 px-6 pb-5 text-[11px] text-muted-foreground sm:pb-6">
        <span>{ta('copyright')}</span>
        <span aria-hidden="true" className="hidden h-3 w-px bg-border sm:block" />
        <span className="inline-flex items-center gap-1.5">
          <ShieldCheck aria-hidden="true" className="size-3.5" />
          {ta('secureLabel')}
        </span>
      </footer>
    </main>
  )
}
