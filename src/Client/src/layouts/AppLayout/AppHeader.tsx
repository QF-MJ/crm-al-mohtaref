import { Link } from 'react-router-dom'
import { Languages, Menu, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import logo from '@/assets/brand/logo.png'
import { paths } from '@/app/router/paths'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { useAuthStore } from '@/stores/auth.store'
import { AccessBadge } from '@/components/common/AccessBadge'
import { ThemeToggle } from '@/components/common/ThemeToggle'

interface AppHeaderProps {
  className?: string
  navigationExpanded: boolean
  onToggleNavigation: () => void
}

export function AppHeader({ className, navigationExpanded, onToggleNavigation }: AppHeaderProps) {
  const { t, i18n } = useTranslation()
  const user = useAuthStore((state) => state.user)
  const nextLanguage = i18n.language.startsWith('ar') ? 'en' : 'ar'

  return (
    <header
      className={cn(
        'sticky top-0 z-30 flex h-[4.5rem] shrink-0 items-center justify-between gap-3 border-b border-slate-200/80 bg-white/95 px-4 backdrop-blur sm:px-6',
        className,
      )}
    >
      <div className="flex min-w-0 items-center gap-3">
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className="lg:hidden"
          onClick={onToggleNavigation}
          aria-label={
            navigationExpanded ? t('actions.closeNavigation') : t('actions.openNavigation')
          }
          aria-expanded={navigationExpanded}
          aria-controls="app-navigation"
        >
          {navigationExpanded ? <X aria-hidden="true" /> : <Menu aria-hidden="true" />}
        </Button>
        <Link
          to={paths.home}
          aria-label={t('appName')}
          className="rounded-md outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
          <img src={logo} alt="" className="app-logo h-9 w-auto" />
        </Link>
        <span className="hidden h-8 w-px bg-slate-200 sm:block" />
        <div className="hidden min-w-0 sm:block">
          <p className="truncate text-sm font-semibold text-slate-800">{t('appName')}</p>
          <p className="truncate text-xs text-slate-500" dir="auto">
            {user?.fullName}
          </p>
        </div>
      </div>

      <div className="flex items-center gap-2">
        <div className="hidden md:block">
          <AccessBadge />
        </div>
        <ThemeToggle />
        <Button
          type="button"
          variant="ghost"
          size="sm"
          onClick={() => void i18n.changeLanguage(nextLanguage)}
          aria-label={t('actions.switchLanguage')}
          className="gap-2 text-slate-600"
        >
          <Languages aria-hidden="true" />
          <span className="hidden sm:inline">
            {nextLanguage === 'ar' ? t('actions.arabic') : t('actions.english')}
          </span>
        </Button>
        <Link
          to={paths.account}
          aria-label={t('nav.account')}
          className="grid size-10 place-items-center rounded-full bg-teal-50 text-sm font-bold text-teal-800 outline-none ring-1 ring-inset ring-teal-700/10 hover:bg-teal-100 focus-visible:ring-2 focus-visible:ring-ring"
        >
          {user?.fullName?.trim().charAt(0) || '؟'}
        </Link>
      </div>
    </header>
  )
}
