import { Link } from 'react-router-dom'
import { Bell } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import logo from '@/assets/brand/logo.png'
import { paths } from '@/app/router/paths'
import { cn } from '@/lib/utils'

interface AppHeaderProps {
  unreadCount?: number
  className?: string
}

export function AppHeader({ unreadCount = 0, className }: AppHeaderProps) {
  const { t } = useTranslation()
  const bellLabel =
    unreadCount > 0 ? `${t('nav.notifications')} (${unreadCount})` : t('nav.notifications')

  return (
    <header className={cn('flex h-16 shrink-0 items-center justify-between border-b bg-white px-6', className)}>
      <Link
        to={paths.home}
        aria-label={t('appName')}
        className="rounded-md outline-none focus-visible:ring-2 focus-visible:ring-ring"
      >
        <img src={logo} alt="" className="h-10 w-auto" />
      </Link>

      <Link
        to={paths.notifications}
        aria-label={bellLabel}
        className="relative grid size-11 place-items-center rounded-full text-primary outline-none transition-colors hover:bg-muted focus-visible:ring-2 focus-visible:ring-ring"
      >
        <Bell className="size-6" aria-hidden="true" />
        {unreadCount > 0 && (
          <span
            aria-hidden="true"
            className="absolute end-1 top-1 grid h-5 min-w-5 place-items-center rounded-full bg-destructive px-1 text-xs font-bold text-white"
          >
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </Link>
    </header>
  )
}