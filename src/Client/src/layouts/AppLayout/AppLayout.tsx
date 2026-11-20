import { useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { LogOut } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useLogout } from '@/features/auth'
import { usePermission } from '@/permissions/usePermission'
import { useAuthStore } from '@/stores/auth.store'
import { useLogout } from '@/features/auth/api/auth.queries'
import { AppHeader } from './AppHeader'
import { navItems } from './nav'

export function AppLayout() {
  const { t } = useTranslation()
  const can = usePermission()
  const user = useAuthStore((s) => s.user)
  const { mutate: logout, isPending } = useLogout()
  const [confirmOpen, setConfirmOpen] = useState(false)

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

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex items-center justify-between border-b px-6 py-3">
          <span className="text-sm text-muted-foreground">{user?.name}</span>
          <Button variant="outline" size="sm" onClick={() => setConfirmOpen(true)}>
            {t('actions.logout')}
          </Button>
        </header>
        <main className="flex-1 p-6">
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
