import { Link, useLocation } from 'react-router-dom'
import { Bell, Languages, Menu, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import logo from '@/assets/brand/logo.png'
import { paths } from '@/app/router/paths'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { useAuthStore } from '@/stores/auth.store'
import { navItems } from './nav'

interface AppHeaderProps {
  unreadCount?: number
  className?: string
  navigationExpanded: boolean
  onToggleNavigation: () => void
}

export function AppHeader({
  unreadCount = 0,
  className,
  navigationExpanded,
  onToggleNavigation,
}: AppHeaderProps) {
  const { t, i18n } = useTranslation()
  const location = useLocation()
  const user = useAuthStore((state) => state.user)
  const bellLabel =
    unreadCount > 0 ? `${t('nav.notifications')} (${unreadCount})` : t('nav.notifications')
  const currentPage = navItems.find(
    (item) => location.pathname === item.to || location.pathname.startsWith(`${item.to}/`),
  )
  const nextLanguage = i18n.language.startsWith('ar') ? 'en' : 'ar'

  return (
    <header
      className={cn(
        'sticky top-0 z-30 flex h-[4.5rem] shrink-0 items-center justify-between border-b border-slate-200/80 bg-white/95 px-4 backdrop-blur sm:px-6',
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
          aria-label={navigationExpanded ? t('actions.closeNavigation') : t('actions.openNavigation')}
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
          <img src={logo} alt="" className="h-9 w-auto" />
        </Link>
        <span className="hidden h-8 w-px bg-slate-200 sm:block" />
        <div className="hidden min-w-0 sm:block">
          <p className="truncate text-sm font-semibold text-slate-800">
            {currentPage ? t(currentPage.labelKey) : t('appName')}
          </p>
          <p className="truncate text-xs text-slate-500">{user?.name}</p>
        </div>
      </div>

      <div className="flex items-center gap-1">
        <Button
          type="button"
          variant="ghost"
          size="sm"
          onClick={() => void i18n.changeLanguage(nextLanguage)}
          aria-label={t('actions.switchLanguage')}
          className="gap-2 text-slate-600"
        >
          <Languages aria-hidden="true" />
          <span>{nextLanguage === 'ar' ? t('actions.arabic') : t('actions.english')}</span>
        </Button>
        <Link
          to={paths.notifications}
          aria-label={bellLabel}
          className="relative grid size-10 place-items-center rounded-xl text-slate-600 outline-none transition-colors hover:bg-slate-100 focus-visible:ring-2 focus-visible:ring-ring"
        >
          <Bell className="size-5" aria-hidden="true" />
          {unreadCount > 0 && (
            <span
              aria-hidden="true"
              className="absolute end-1 top-1 grid h-4 min-w-4 place-items-center rounded-full bg-destructive px-1 text-[10px] font-bold text-white"
            >
              {unreadCount > 99 ? '99+' : unreadCount}
            </span>
          )}
        </Link>
      </div>
    </header>
  )
}