import { useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { LogOut } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useLogout } from '@/features/auth'
import { useAccess } from '@/permissions/useAccess'
import { useAuthStore } from '@/stores/auth.store'
import { AppHeader } from './AppHeader'
import { navItems } from './nav'

export function AppLayout() {
  const { t } = useTranslation()
  const access = useAccess()
  const user = useAuthStore((state) => state.user)
  const { mutate: logout, isPending } = useLogout()
  const [navigationExpanded, setNavigationExpanded] = useState(false)
  const [confirmOpen, setConfirmOpen] = useState(false)

  const visibleItems = navItems.filter((item) => access.can(item.capability))
  const roleSummary = access.pairs
    .map((pair) => `${pair.roleName} · ${pair.departmentName}`)
    .join(' | ')

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
          <div className="mb-6 rounded-xl border border-slate-200 bg-slate-50 p-4">
            <p className="text-xs font-medium text-slate-500">{t('navigation.workspace')}</p>
            <p className="mt-1 truncate text-sm font-semibold text-slate-900" dir="auto">
              {user?.fullName || t('account.unknownUser')}
            </p>
            <p className="mt-0.5 line-clamp-2 text-xs leading-5 text-slate-500" dir="auto">
              {roleSummary || t('permissions.none')}
            </p>
          </div>

          <nav aria-label={t('navigation.main')} className="flex flex-1 flex-col gap-1.5">
            {visibleItems.map((item) => {
              const Icon = item.icon
              return (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.to === '/'}
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
                </NavLink>
              )
            })}
          </nav>

          <div className="mt-6 border-t border-slate-100 pt-4">
            <Button
              variant="ghost"
              size="default"
              className="w-full justify-start gap-3 rounded-xl text-slate-600 hover:bg-rose-50 hover:text-rose-700"
              onClick={() => setConfirmOpen(true)}
            >
              <LogOut aria-hidden="true" />
              {t('actions.logout')}
            </Button>
          </div>
        </aside>

        <main id="main-content" className="min-w-0 flex-1 p-4 sm:p-6 xl:p-8">
          <Outlet />
        </main>
      </div>

      {confirmOpen ? (
        <div
          className="fixed inset-0 z-50 grid place-items-center bg-black/40 p-4"
          role="presentation"
          onClick={() => {
            if (!isPending) setConfirmOpen(false)
          }}
        >
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="logout-title"
            aria-describedby="logout-message"
            className="w-full max-w-sm rounded-lg border bg-card p-5 shadow-lg"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="logout-title" className="text-base font-semibold">
              {t('logout.title')}
            </h2>
            <p id="logout-message" className="mt-2 text-sm text-muted-foreground">
              {t('logout.message')}
            </p>
            <div className="mt-5 flex justify-end gap-2">
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => setConfirmOpen(false)}
                disabled={isPending}
              >
                {t('actions.cancel')}
              </Button>
              <Button
                type="button"
                variant="destructive"
                size="sm"
                onClick={() => logout()}
                disabled={isPending}
              >
                {t('logout.confirm')}
              </Button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}
