import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { KeyRound, ShieldCheck, UserRound } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { PageHeader } from '@/components/common/PageHeader'
import { ErrorState } from '@/components/common/ErrorState'
import { paths } from '@/app/router/paths'
import { useAccess } from '@/permissions/useAccess'
import { useAuthStore } from '@/stores/auth.store'
import { useUser } from '../api/users.queries'

// صفحة الحساب تعمل لكل المستخدمين: الباك يسمح للمستخدم بقراءة ملفه
export function AccountPage() {
  const { t } = useTranslation()
  const access = useAccess()
  const sessionUser = useAuthStore((state) => state.user)
  const userQuery = useUser(sessionUser?.id)
  const user = userQuery.data

  const permissions = user?.permissions ?? sessionUser?.permissions ?? []

  return (
    <section className="space-y-6">
      <PageHeader
        breadcrumb={t('account.breadcrumb')}
        title={t('account.title')}
        description={t('account.subtitle')}
      />

      <div className="grid gap-5 lg:grid-cols-[1.3fr_1fr]">
        <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
          <h2 className="flex items-center gap-2 font-semibold text-slate-900">
            <UserRound aria-hidden="true" className="size-4 text-slate-400" />
            {t('account.details')}
          </h2>

          {userQuery.isLoading ? (
            <div className="mt-5 space-y-3" role="status" aria-busy="true">
              {Array.from({ length: 4 }, (_, index) => (
                <div key={index} className="h-10 animate-pulse rounded-lg bg-slate-100" />
              ))}
            </div>
          ) : userQuery.isError ? (
            <ErrorState error={userQuery.error} onRetry={() => void userQuery.refetch()} />
          ) : (
            <dl className="mt-5 grid gap-4 sm:grid-cols-2">
              <DetailItem label={t('users.fullName')} value={user?.fullName} />
              <DetailItem label={t('users.username')} value={user?.accountName} ltr />
              <DetailItem label={t('users.email')} value={user?.email} ltr />
              <div className="space-y-1">
                <dt className="text-xs text-slate-500">{t('users.status')}</dt>
                <dd>
                  {user?.isActive === false ? (
                    <Badge variant="danger">{t('users.inactive')}</Badge>
                  ) : (
                    <Badge variant="success" dot>
                      {t('users.active')}
                    </Badge>
                  )}
                </dd>
              </div>
            </dl>
          )}
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
          <h2 className="flex items-center gap-2 font-semibold text-slate-900">
            <ShieldCheck aria-hidden="true" className="size-4 text-slate-400" />
            {t('account.accessTitle')}
          </h2>
          <p className="mt-1 text-xs leading-6 text-slate-500">{t('account.accessDescription')}</p>
          <p className="mt-4 text-sm font-medium text-slate-700">
            {access.isAdmin
              ? t('permissions.levelAdmin')
              : access.isManager
                ? t('permissions.levelManager')
                : t('permissions.levelEmployee')}
          </p>
          <Link
            to={paths.dashboard}
            className="mt-4 inline-flex text-sm font-medium text-primary hover:underline"
          >
            {t('account.backToDashboard')}
          </Link>
        </section>
      </div>

      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
        <h2 className="flex items-center gap-2 font-semibold text-slate-900">
          <KeyRound aria-hidden="true" className="size-4 text-slate-400" />
          {t('account.permissionsTitle')}
        </h2>
        <p className="mt-1 text-xs leading-6 text-slate-500">
          {t('account.permissionsDescription')}
        </p>

        {permissions.length === 0 ? (
          <p className="mt-4 rounded-xl bg-amber-50 px-4 py-3 text-sm text-amber-800">
            {t('permissions.none')}
          </p>
        ) : (
          <ul className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
            {permissions.map((permission) => (
              <li
                key={`${permission.id}-${permission.departmentName}-${permission.roleName}`}
                className="rounded-xl border border-slate-200 bg-slate-50/60 p-4"
              >
                <p className="text-sm font-semibold text-slate-800">{permission.roleName}</p>
                <p className="mt-1 text-xs text-slate-500">
                  {t('users.department')}: {permission.departmentName}
                </p>
              </li>
            ))}
          </ul>
        )}
      </section>
    </section>
  )
}

function DetailItem({
  label,
  value,
  ltr = false,
}: {
  label: string
  value?: string
  ltr?: boolean
}) {
  return (
    <div className="space-y-1">
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd
        className="truncate text-sm font-medium text-slate-800"
        dir={ltr ? 'ltr' : 'auto'}
        title={value}
      >
        {value?.trim() ? value : '—'}
      </dd>
    </div>
  )
}
