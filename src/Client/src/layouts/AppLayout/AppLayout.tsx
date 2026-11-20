import { useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { useLogout } from '@/features/auth'
import { usePermission } from '@/permissions/usePermission'
import { useAuthStore } from '@/stores/auth.store'
import { navItems } from './nav'

export function AppLayout() {
  const { t } = useTranslation()
  const can = usePermission()
  const user = useAuthStore((s) => s.user)
  const { mutate: logout, isPending } = useLogout()
  const [confirmOpen, setConfirmOpen] = useState(false)

  return (
    <div className="flex min-h-screen">
      {/* الشريط الجانبي: ps/pe/border-e بدل left/right ليتبع الاتجاه */}
      <aside className="w-60 shrink-0 border-e bg-card p-4">
        <h1 className="mb-6 text-lg font-bold">{t('appName')}</h1>
        <nav className="flex flex-col gap-1">
          {navItems
            .filter((item) => can(item.permission))
            .map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `rounded-md px-3 py-2 text-sm ${
                    isActive ? 'bg-primary text-primary-foreground' : 'hover:bg-muted'
                  }`
                }
              >
                {t(item.labelKey)}
              </NavLink>
            ))}
        </nav>
      </aside>

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
