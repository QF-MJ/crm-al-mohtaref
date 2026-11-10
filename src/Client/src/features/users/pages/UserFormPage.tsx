import { useMemo } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { paths } from '@/app/router/paths'
import { useUpdateUser, useUser, useUserLookups } from '../api/users.queries'
import { UserForm } from '../components/UserForm'
import type { UserFormValues } from '../schemas/user.schema'
import type { UserDetail, UserLookups } from '../types'

function getErrorMessage(error: unknown) {
  if (typeof error === 'object' && error !== null && 'message' in error) {
    return String(error.message)
  }
  return ''
}

function getDefaultValues(user: UserDetail, lookups: UserLookups) {
  const permissions: UserFormValues['permissions'] = []
  for (const permission of user.permissions) {
    const role = lookups.roles.find(
      (item) =>
        item.name.localeCompare(permission.roleName, undefined, { sensitivity: 'base' }) === 0,
    )
    const department = lookups.departments.find((item) =>
      item.name.localeCompare(permission.departmentName, undefined, { sensitivity: 'base' }) === 0,
    )
    if (!role || !department) return undefined
    permissions.push({ roleId: role.id, departmentId: department.id })
  }

  return {
    accountName: user.accountName,
    fullName: user.fullName,
    email: user.email,
    isActive: user.isActive,
    permissions,
  }
}

export function UserFormPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { userId: userIdParam } = useParams()
  const userId = userIdParam && /^\d+$/.test(userIdParam) ? Number(userIdParam) : undefined
  const userQuery = useUser(userId)
  const lookupsQuery = useUserLookups()
  const updateMutation = useUpdateUser(userId ?? 0)
  const defaultValues = useMemo(() => {
    if (!userQuery.data || !lookupsQuery.data) return undefined
    return getDefaultValues(userQuery.data, lookupsQuery.data)
  }, [userQuery.data, lookupsQuery.data])

  const submit = (values: UserFormValues) => {
    if (userId === undefined) return
    updateMutation.mutate(
      {
        accountName: values.accountName,
        fullName: values.fullName,
        email: values.email,
        isActive: values.isActive,
        permissions: values.permissions,
      },
      {
        onSuccess: () => {
          toast.success(t('users.updateSuccess'))
          navigate(paths.users)
        },
        onError: (error) => toast.error(getErrorMessage(error) || t('users.unknownError')),
      },
    )
  }

  if (userId === undefined) {
    return <UserPageError message={t('users.notFound')} />
  }

  if (userQuery.isLoading || lookupsQuery.isLoading) {
    return <p className="p-8 text-center text-sm text-slate-500">{t('users.loading')}</p>
  }

  if (userQuery.isError || lookupsQuery.isError || !userQuery.data || !lookupsQuery.data) {
    const error = userQuery.error ?? lookupsQuery.error
    return (
      <UserPageError message={getErrorMessage(error) || t('users.unknownError')} />
    )
  }

  if (!defaultValues) {
    return <UserPageError message={t('users.permissionLookupError')} />
  }

  return (
    <section className="mx-auto max-w-3xl space-y-5" dir="rtl">
      <header>
        <p className="mb-1 text-xs text-slate-400">{t('users.breadcrumb')}</p>
        <h1 className="text-2xl font-bold text-slate-800">{t('users.editTitle')}</h1>
        <p className="mt-1 text-sm text-slate-500">{t('users.formDescription')}</p>
      </header>
      <UserForm
        defaultValues={defaultValues}
        departments={lookupsQuery.data.departments}
        roles={lookupsQuery.data.roles}
        isPending={updateMutation.isPending}
        onSubmit={submit}
      />
    </section>
  )
}

function UserPageError({ message }: { message: string }) {
  const { t } = useTranslation()
  return (
    <section
      className="mx-auto max-w-3xl space-y-4 rounded-xl border bg-white p-6 text-center"
      role="alert"
    >
      <p className="text-sm text-destructive">{message}</p>
      <Link
        to={paths.users}
        className="inline-flex h-8 items-center justify-center rounded-md border bg-white px-3 text-sm font-medium hover:bg-accent"
      >
        {t('users.backToList')}
      </Link>
    </section>
  )
}
