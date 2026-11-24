import { Link, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ArrowRight, KeyRound, Pencil, UserRound } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { PageHeader } from '@/components/common/PageHeader'
import { ErrorState } from '@/components/common/ErrorState'
import { paths } from '@/app/router/paths'
import { useAccess } from '@/permissions/useAccess'
import { useAuthStore } from '@/stores/auth.store'
import { useUser } from '../api/users.queries'

export function UserDetailPage() {
  const { t } = useTranslation()
  const { userId } = useParams<{ userId: string }>()
  const access = useAccess()
  const sessionUser = useAuthStore((state) => state.user)
  const userQuery = useUser(userId)

  if (!userId) {
    return (
      <section className="space-y-4">
        <ErrorState error={{ status: 404, message: t('users.notFound') }} />
        <BackLink />
      </section>
    )
  }

  if (userQuery.isLoading) {
    return (
      <section className="space-y-5">
        <PageHeader breadcrumb={t('users.breadcrumb')} title={t('users.detailTitle')} />
        <div
          className="space-y-3 rounded-2xl border border-slate-200 bg-white p-6"
          aria-busy="true"
        >
          {Array.from({ length: 5 }, (_, index) => (
            <div key={index} className="h-10 animate-pulse rounded-lg bg-slate-100" />
          ))}
        </div>
      </section>
    )
  }

  if (userQuery.isError || !userQuery.data) {
    return (
      <section className="space-y-5">
        <PageHeader breadcrumb={t('users.breadcrumb')} title={t('users.detailTitle')} />
        <div className="rounded-2xl border border-slate-200 bg-white">
          <ErrorState error={userQuery.error} onRetry={() => void userQuery.refetch()} />
        </div>
        <BackLink />
      </section>
    )
  }

  const user = userQuery.data
  const isSelf = sessionUser?.id === user.id

  return (
    <section className="space-y-5">
      <PageHeader
        breadcrumb={t('users.breadcrumb')}
        title={user.fullName?.trim() || user.accountName}
        description={isSelf ? t('users.selfNote') : t('users.detailSubtitle')}
        actions={
          access.can('users.update') ? (
            <Link
              to={paths.userEdit(user.id)}
              className="inline-flex h-9 items-center gap-2 rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground hover:bg-primary/90"
            >
              <Pencil aria-hidden="true" className="size-4" />
              {t('actions.edit')}
            </Link>
          ) : (
            <Badge variant="neutral">{t('users.readOnly')}</Badge>
          )
        }
      />

      <div className="grid gap-5 lg:grid-cols-[1.2fr_1fr]">
        <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
          <h2 className="flex items-center gap-2 font-semibold text-slate-900">
            <UserRound aria-hidden="true" className="size-4 text-slate-400" />
            {t('users.accountDetails')}
          </h2>
          <dl className="mt-4 grid gap-4 sm:grid-cols-2">
            <DetailItem label={t('users.fullName')} value={user.fullName} />
            <DetailItem label={t('users.username')} value={user.accountName} ltr />
            <DetailItem label={t('users.email')} value={user.email} ltr />
            <div className="space-y-1">
              <dt className="text-xs text-slate-500">{t('users.status')}</dt>
              <dd>
                {user.isActive ? (
                  <Badge variant="success" dot>
                    {t('users.active')}
                  </Badge>
                ) : (
                  <Badge variant="danger" dot>
                    {t('users.inactive')}
                  </Badge>
                )}
              </dd>
            </div>
          </dl>
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
          <h2 className="flex items-center gap-2 font-semibold text-slate-900">
            <KeyRound aria-hidden="true" className="size-4 text-slate-400" />
            {t('users.permissionsTitle')}
          </h2>
          <p className="mt-1 text-xs leading-6 text-slate-500">
            {t('users.permissionsReadOnlyHint')}
          </p>

          {user.permissions.length === 0 ? (
            <p className="mt-4 rounded-xl bg-amber-50 px-4 py-3 text-sm text-amber-800">
              {t('users.noPermissions')}
            </p>
          ) : (
            <ul className="mt-4 space-y-2">
              {user.permissions.map((permission) => (
                <li
                  key={permission.id}
                  className="flex items-center justify-between gap-3 rounded-xl border border-slate-200 bg-slate-50/60 px-4 py-3 text-sm"
                >
                  <span className="font-medium text-slate-800">{permission.roleName}</span>
                  <span className="text-xs text-slate-500">{permission.departmentName}</span>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>

      <BackLink />
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

function BackLink() {
  const { t } = useTranslation()
  return (
    <Link
      to={paths.users}
      className="inline-flex items-center gap-2 text-sm font-medium text-primary hover:underline"
    >
      <ArrowRight aria-hidden="true" className="size-4" />
      {t('users.backToList')}
    </Link>
  )
}
