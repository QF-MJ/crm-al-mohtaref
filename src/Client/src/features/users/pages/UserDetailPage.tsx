import { Link, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { paths } from '@/app/router/paths'
import { usePermission } from '@/permissions/usePermission'
import { getApiErrorMessage } from '@/api/errors'
import { useUser } from '../api/users.queries'
import type { UserPermission } from '../types'

export function UserDetailPage() {
  const { t } = useTranslation()
  const { userId: userIdParam } = useParams()
  const userId = userIdParam || undefined
  const userQuery = useUser(userId)
  const can = usePermission()

  if (userId === undefined) {
    return <UserDetailError message={t('users.notFound')} />
  }
  if (userQuery.isLoading) {
    return (
      <p className="p-8 text-center text-sm text-slate-500" role="status">
        {t('users.loading')}
      </p>
    )
  }
  if (userQuery.isError || !userQuery.data) {
    return (
      <section
        className="mx-auto max-w-3xl space-y-4 rounded-xl border bg-white p-6 text-center"
        role="alert"
      >
        <p className="text-sm text-destructive">
          {getApiErrorMessage(userQuery.error, t('users.unknownError'))}
        </p>
        <Button
          type="button"
          variant="outline"
          onClick={() => void userQuery.refetch()}
          disabled={userQuery.isFetching}
        >
          {t('users.retry')}
        </Button>
        <Link
          to={paths.users}
          className="block text-sm font-medium text-primary hover:underline"
        >
          {t('users.backToList')}
        </Link>
      </section>
    )
  }

  const user = userQuery.data
  return (
    <section className="-m-6 min-h-full space-y-5 bg-slate-50 p-5 sm:p-6" dir="rtl">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="mb-1 text-xs text-slate-400">{t('users.breadcrumb')}</p>
          <h1 className="text-2xl font-bold text-slate-800">
            {user.fullName ?? user.accountName}
          </h1>
        </div>
        {can('users.suspend') && (
          <Link
            to={paths.userEdit(String(user.id))}
            className="inline-flex h-9 items-center rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground"
          >
            {t('users.edit')}
          </Link>
        )}
      </header>

      <section className="space-y-4 rounded-xl border bg-white p-5 sm:p-6">
        <h2 className="font-semibold">{t('users.accountDetails')}</h2>
        <dl className="grid gap-4 sm:grid-cols-2">
          <DetailItem label={t('users.username')} value={user.accountName} />
          <DetailItem label={t('users.name')} value={user.fullName ?? '—'} />
          <DetailItem label={t('users.email')} value={user.email ?? '—'} />
          <DetailItem
            label={t('users.status')}
            value={
              user.isActive === undefined
                ? '—'
                : t(user.isActive ? 'users.active' : 'users.inactive')
            }
          />
        </dl>
      </section>

      <section className="space-y-4 rounded-xl border bg-white p-5 sm:p-6">
        <h2 className="font-semibold">{t('users.permissionsTitle')}</h2>
        {!user.permissions?.length ? (
          <p className="text-sm text-slate-500">{t('users.noPermissions')}</p>
        ) : (
          <ul className="grid gap-3 sm:grid-cols-2">
            {user.permissions.map((permission) => (
              <PermissionItem key={permission.id} permission={permission} />
            ))}
          </ul>
        )}
      </section>

      <Link to={paths.users} className="inline-flex text-sm font-medium text-primary hover:underline">
        {t('users.backToList')}
      </Link>
    </section>
  )
}

function PermissionItem({ permission }: { permission: UserPermission }) {
  const { t } = useTranslation()
  return (
    <li className="rounded-md border p-3 text-sm">
      <p className="text-xs text-slate-500">
        {t('users.permissionId')}: {permission.id}
      </p>
      <p className="mt-1">
        <span className="font-medium">{permission.roleName}</span>
        <span className="mx-2 text-slate-400">/</span>
        <span>{permission.departmentName}</span>
      </p>
    </li>
  )
}

function DetailItem({ label, value }: { label: string; value: string }) {
  return (
    <div className="space-y-1">
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd className="text-sm font-medium text-slate-800" dir="auto">
        {value}
      </dd>
    </div>
  )
}

function UserDetailError({ message }: { message: string }) {
  const { t } = useTranslation()
  return (
    <section
      className="mx-auto max-w-3xl space-y-4 rounded-xl border bg-white p-6 text-center"
      role="alert"
    >
      <p className="text-sm text-destructive">{message}</p>
      <Link
        to={paths.users}
        className="inline-flex text-sm font-medium text-primary hover:underline"
      >
        {t('users.backToList')}
      </Link>
    </section>
  )
}
