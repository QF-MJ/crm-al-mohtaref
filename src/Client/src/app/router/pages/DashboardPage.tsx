import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { BriefcaseBusiness, ShieldCheck, UserRound, UsersRound } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { PageHeader } from '@/components/common/PageHeader'
import { paths } from '@/app/router/paths'
import { useAccess } from '@/permissions/useAccess'
import type { Capability } from '@/permissions/access'
import { useAuthStore } from '@/stores/auth.store'

interface Shortcut {
  titleKey: string
  descriptionKey: string
  to: string
  capability: Capability
  icon: typeof UsersRound
}

const SHORTCUTS: Shortcut[] = [
  {
    titleKey: 'dashboard.clientsCard.title',
    descriptionKey: 'dashboard.clientsCard.description',
    to: paths.clients,
    capability: 'contacts.view',
    icon: BriefcaseBusiness,
  },
  {
    titleKey: 'dashboard.usersCard.title',
    descriptionKey: 'dashboard.usersCard.description',
    to: paths.users,
    capability: 'users.view',
    icon: UsersRound,
  },
  {
    titleKey: 'dashboard.accountCard.title',
    descriptionKey: 'dashboard.accountCard.description',
    to: paths.account,
    capability: 'account.view',
    icon: UserRound,
  },
]

export function DashboardPage() {
  const { t } = useTranslation()
  const access = useAccess()
  const user = useAuthStore((state) => state.user)
  const shortcuts = SHORTCUTS.filter((shortcut) => access.can(shortcut.capability))

  return (
    <section className="space-y-6">
      <PageHeader
        breadcrumb={t('dashboard.breadcrumb')}
        title={t('dashboard.greeting', { name: user?.fullName || t('account.unknownUser') })}
        description={t('dashboard.subtitle')}
        actions={
          access.isAdmin ? (
            <Badge variant="success" dot>
              <ShieldCheck aria-hidden="true" className="size-3.5" />
              {t('permissions.admin')}
            </Badge>
          ) : null
        }
      />

      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
        <div className="grid gap-6 lg:grid-cols-[1.35fr_1fr]">
          <div>
            <h2 className="font-semibold text-slate-900">{t('dashboard.workspaceTitle')}</h2>
            <p className="mt-1 text-sm leading-6 text-slate-500">
              {t('dashboard.workspaceDescription')}
            </p>

            <div className="mt-4 flex flex-wrap gap-2">
              {access.pairs.length === 0 ? (
                <Badge variant="warning">{t('permissions.none')}</Badge>
              ) : (
                access.pairs.map((pair) => (
                  <Badge key={`${pair.departmentName}-${pair.roleName}-${pair.id}`} variant="info">
                    {pair.roleName} · {pair.departmentName}
                  </Badge>
                ))
              )}
            </div>
          </div>

          <dl className="grid content-start gap-3 sm:grid-cols-2 lg:grid-cols-1">
            <div className="rounded-xl border border-slate-200/70 bg-slate-50 p-4">
              <dt className="text-xs text-slate-500">{t('dashboard.scope')}</dt>
              <dd className="mt-1 text-sm font-medium text-slate-800">
                {access.isAdmin
                  ? t('dashboard.scopeAll')
                  : access.managerDepartments.length > 0
                    ? access.managerDepartments.join('، ')
                    : t('dashboard.scopeOwn')}
              </dd>
            </div>
            <div className="rounded-xl border border-slate-200/70 bg-slate-50 p-4">
              <dt className="text-xs text-slate-500">{t('dashboard.accessLevel')}</dt>
              <dd className="mt-1 text-sm font-medium text-slate-800">
                {access.isAdmin
                  ? t('permissions.levelAdmin')
                  : access.isManager
                    ? t('permissions.levelManager')
                    : t('permissions.levelEmployee')}
              </dd>
            </div>
          </dl>
        </div>
      </section>

      <section className="space-y-3">
        <h2 className="font-semibold text-slate-900">{t('dashboard.quickAccess')}</h2>
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {shortcuts.map((shortcut) => {
            const Icon = shortcut.icon
            return (
              <Link
                key={shortcut.to}
                to={shortcut.to}
                className="flex items-start gap-3 rounded-xl border border-slate-200 bg-white p-4 transition-colors hover:border-slate-300 hover:bg-slate-50"
              >
                <Icon aria-hidden="true" className="mt-0.5 size-5 shrink-0 text-teal-700" />
                <span className="min-w-0">
                  <span className="block text-sm font-semibold text-slate-900">
                    {t(shortcut.titleKey)}
                  </span>
                  <span className="mt-1 block text-xs leading-6 text-slate-500">
                    {t(shortcut.descriptionKey)}
                  </span>
                </span>
              </Link>
            )
          })}
        </div>
      </section>
    </section>
  )
}
