import { NavLink, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { usePermission } from '@/permissions/usePermission'
import { useAuthStore } from '@/stores/auth.store'
import { useLogout } from '@/features/auth/api/auth.queries'
import { AppHeader } from './AppHeader'
import { navItems } from './nav'

export function AppLayout() {
  const { t } = useTranslation()
  const can = usePermission()
  const user = useAuthStore((s) => s.user)
  const logout = useLogout()

  const handleLogout = () => logout.mutate()

  return (
    <div className="flex min-h-screen flex-col">
      <AppHeader />
      <div className="flex flex-1">
        {/* border-e بدل border-r ليتبع اتجاه الصفحة */}
        <aside className="flex w-60 shrink-0 flex-col border-e bg-card p-4">
          <nav className="flex flex-1 flex-col gap-1">
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
          <div className="border-t pt-4">
            <p className="mb-2 text-sm text-muted-foreground">{user?.name}</p>
            <Button
              variant="outline"
              size="sm"
              className="w-full"
              onClick={handleLogout}
              disabled={logout.isPending}
            >
              {t('actions.logout')}
            </Button>
          </div>
        </aside>
        <main className="min-w-0 flex-1 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}