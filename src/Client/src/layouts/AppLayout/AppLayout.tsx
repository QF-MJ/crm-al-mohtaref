import { useState } from 'react'
import { NavLink, Outlet, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { LogOut } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { usePermission } from '@/permissions/usePermission'
import { useAuthStore } from '@/stores/auth.store'
import { useLogout } from '@/features/auth/api/auth.queries'
import { toast } from 'sonner'
import { AppHeader } from './AppHeader'
import { navItems } from './nav'

export function AppLayout() {
  const { t } = useTranslation()
  const location = useLocation()
  const can = usePermission()
  const user = useAuthStore((s) => s.user)
  const logout = useLogout()
  const [navigationExpanded, setNavigationExpanded] = useState(false)

  const handleLogout = () =>
    logout.mutate(undefined, {
      onError: () => toast.error(t('errors.logoutFailed')),
    })

  return (
    <div className="min-h-screen bg-[#f5f7f8] text-slate-900">
      <AppHeader
        navigationExpanded={navigationExpanded}
        onToggleNavigation={() => setNavigationExpanded((expanded) => !expanded)}
      />
      <div className="mx-auto flex min-h-[calc(100vh-4.5rem)] max-w-[1680px]">
        <aside
          id="app-navigation"
          className={`${
            navigationExpanded ? 'flex' : 'hidden'
          } fixed inset-x-0 top-[4.5rem] z-20 max-h-[calc(100vh-4.5rem)] flex-col overflow-y-auto border-b border-slate-200 bg-white p-4 shadow-xl lg:sticky lg:top-[4.5rem] lg:flex lg:h-[calc(100vh-4.5rem)] lg:w-64 lg:shrink-0 lg:border-b-0 lg:border-e lg:shadow-none`}
        >
          <div className="mb-6 rounded-2xl bg-gradient-to-br from-teal-800 to-slate-800 p-4 text-white">
            <p className="text-xs font-medium text-teal-100">{t('navigation.workspace')}</p>
            <p className="mt-1 truncate text-base font-semibold">{user?.name || user?.accountName}</p>
            <p className="mt-1 truncate text-xs text-white/65">{user?.accountName}</p>
          </div>
          <nav aria-label={t('navigation.main')} className="flex flex-1 flex-col gap-1.5">
            {navItems.filter((item) => can(item.permission)).map((item) => {
              const Icon = item.icon
              return (
                <NavLink
                  key={item.to}
                  to={item.to}
                  onClick={() => setNavigationExpanded(false)}
                  className={({ isActive }) =>
                    `group flex items-center gap-3 rounded-xl px-3.5 py-3 text-sm font-medium transition-colors ${
                      isActive
                        ? 'bg-teal-50 text-teal-800 ring-1 ring-inset ring-teal-700/10'
                        : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900'
                    }`
                  }
                >
                  <Icon aria-hidden="true" className="size-[18px] shrink-0 opacity-80" />
                  <span className="flex-1">{t(item.labelKey)}</span>
                  {location.pathname === item.to && (
                    <span aria-hidden="true" className="size-1.5 rounded-full bg-teal-700" />
                  )}
                </NavLink>
              )
            })}
          </nav>
          <div className="mt-6 border-t border-slate-100 pt-4">
            <Button
              variant="ghost"
              size="default"
              className="w-full justify-start gap-3 rounded-xl text-slate-600 hover:bg-rose-50 hover:text-rose-700"
              onClick={handleLogout}
              disabled={logout.isPending}
            >
              <LogOut aria-hidden="true" />
              {logout.isPending ? t('actions.loggingOut') : t('actions.logout')}
            </Button>
          </div>
        </aside>
        <main id="main-content" className="min-w-0 flex-1 p-4 sm:p-6 xl:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}