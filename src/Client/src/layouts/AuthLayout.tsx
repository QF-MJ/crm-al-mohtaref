import { Navigate, Outlet } from 'react-router-dom'
import { ShieldCheck } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { ThemeToggle } from '@/components/common/ThemeToggle'
import { useAuthStore } from '@/stores/auth.store'

export function AuthLayout() {
  const { t } = useTranslation('auth')
  const user = useAuthStore((s) => s.user)
  if (user) return <Navigate to="/" replace />

  return (
    <main className="grid min-h-screen place-items-center bg-[#f1f6f5] p-4 sm:p-8">
      <div className="grid min-h-[640px] w-full max-w-6xl overflow-hidden rounded-[2rem] bg-white shadow-[0_30px_90px_-40px_rgba(15,55,60,0.35)] lg:min-h-[min(760px,calc(100vh-4rem))] lg:grid-cols-[0.92fr_1.08fr]">
        <section className="relative isolate flex min-h-52 flex-col justify-between overflow-hidden bg-gradient-to-br from-[#0b6e6e] via-[#105e67] to-[#173d54] p-7 text-white sm:p-10 lg:p-12">
          <div
            aria-hidden="true"
            className="absolute -end-24 -top-32 -z-10 size-96 rounded-full border border-white/10"
          />
          <div
            aria-hidden="true"
            className="absolute -bottom-40 -start-24 -z-10 size-[30rem] rounded-full border border-white/10"
          />
          <div
            aria-hidden="true"
            className="absolute bottom-16 end-10 -z-10 size-40 rounded-full bg-teal-300/10 blur-3xl"
          />

          <div className="flex items-center gap-4">
            <img
              src="/vertical%20logo%20white.svg"
              alt={t('logoAlt')}
              className="h-14 w-auto object-contain object-right sm:h-[4.5rem]"
            />
            <span className="h-10 w-px bg-white/25" />
            <span className="text-sm font-medium leading-6 text-white/85">{t('systemName')}</span>
          </div>

          <div className="my-8 max-w-md space-y-5 lg:my-0">
            <span className="inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-3 py-1.5 text-xs font-medium text-teal-50">
              <ShieldCheck aria-hidden="true" className="size-4" />
              {t('secureAccess')}
            </span>
            <h2 className="text-3xl font-bold leading-relaxed sm:text-4xl">{t('heroTitle')}</h2>
            <p className="max-w-sm text-sm leading-8 text-teal-50/80 sm:text-base">
              {t('heroDescription')}
            </p>
          </div>

          <p className="hidden text-xs text-white/55 lg:block">{t('copyright')}</p>
        </section>

        <section className="relative flex items-center justify-center px-6 py-10 sm:px-12 lg:px-16">
          {/* متاح قبل تسجيل الدخول أيضاً */}
          <ThemeToggle className="absolute end-3 top-3 z-10" />
          <div className="w-full">
            <Outlet />
          </div>
        </section>
      </div>
    </main>
  )
}
