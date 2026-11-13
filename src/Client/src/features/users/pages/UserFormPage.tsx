import { useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { AlertTriangle } from 'lucide-react'
import { PageHeader } from '@/components/common/PageHeader'
import { ErrorState } from '@/components/common/ErrorState'
import { paths } from '@/app/router/paths'
import { describeApiError } from '@/api/errors'
import { useUpdateUser, useUser, useUserLookups } from '../api/users.queries'
import { UserForm } from '../components/UserForm'
import type { UserFormValues } from '../schemas/user.schema'
import type { UserDetail, UserLookups } from '../types'

interface MappedPermissions {
  permissions: UserFormValues['permissions']
  unmatched: string[]
}

const normalizeName = (value: string) => value.trim().replace(/\s+/g, ' ').toLocaleLowerCase('ar')

// الباك يرسل أسماء القسم والدور في تفاصيل المستخدم، والقوائم ترسل المعرّفات:
// نطابق بالأسماء لنعيد بناء الحقول القابلة للتعديل.
function mapPermissions(user: UserDetail, lookups: UserLookups): MappedPermissions {
  const permissions: UserFormValues['permissions'] = []
  const unmatched: string[] = []

  for (const permission of user.permissions) {
    const role = lookups.roles.find(
      (item) => normalizeName(item.name) === normalizeName(permission.roleName),
    )
    const department = lookups.departments.find(
      (item) => normalizeName(item.name) === normalizeName(permission.departmentName),
    )

    if (!role || !department) {
      unmatched.push(`${permission.roleName} · ${permission.departmentName}`)
      continue
    }

    permissions.push({ roleId: String(role.id), departmentId: String(department.id) })
  }

  return { permissions, unmatched }
}

export function UserFormPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { userId } = useParams<{ userId: string }>()
  const userQuery = useUser(userId)
  const lookupsQuery = useUserLookups()
  const updateMutation = useUpdateUser(userId ?? '')
  const [errorMessage, setErrorMessage] = useState('')

  const mapped = useMemo(() => {
    if (!userQuery.data || !lookupsQuery.data) return undefined
    return mapPermissions(userQuery.data, lookupsQuery.data)
  }, [userQuery.data, lookupsQuery.data])

  const defaultValues = useMemo<UserFormValues | undefined>(() => {
    if (!userQuery.data || !mapped) return undefined
    return {
      accountName: userQuery.data.accountName,
      fullName: userQuery.data.fullName ?? '',
      email: userQuery.data.email ?? '',
      isActive: userQuery.data.isActive ?? false,
      password: undefined,
      permissions: mapped.permissions,
    }
  }, [userQuery.data, mapped])

  if (!userId) {
    return (
      <section className="space-y-5">
        <ErrorState error={{ status: 404, message: t('users.notFound') }} />
      </section>
    )
  }

  if (userQuery.isLoading || lookupsQuery.isLoading) {
    return (
      <section className="space-y-5">
        <PageHeader breadcrumb={t('users.breadcrumb')} title={t('users.editTitle')} />
        <div
          className="space-y-3 rounded-2xl border border-slate-200 bg-white p-6"
          aria-busy="true"
        >
          {Array.from({ length: 6 }, (_, index) => (
            <div key={index} className="h-10 animate-pulse rounded-lg bg-slate-100" />
          ))}
        </div>
      </section>
    )
  }

  if (userQuery.isError || lookupsQuery.isError || !userQuery.data || !lookupsQuery.data) {
    return (
      <section className="space-y-5">
        <PageHeader breadcrumb={t('users.breadcrumb')} title={t('users.editTitle')} />
        <div className="rounded-2xl border border-slate-200 bg-white">
          <ErrorState
            error={userQuery.error ?? lookupsQuery.error}
            onRetry={() => {
              void userQuery.refetch()
              void lookupsQuery.refetch()
            }}
          />
        </div>
        <BackLink />
      </section>
    )
  }

  // لا نحفظ على بيانات ناقصة: حذف صلاحية دون علم المستخدم أسوأ من منع التعديل
  if (!defaultValues || !mapped || mapped.unmatched.length > 0) {
    return (
      <section className="space-y-5">
        <PageHeader breadcrumb={t('users.breadcrumb')} title={t('users.editTitle')} />
        <div className="rounded-2xl border border-amber-200 bg-amber-50 p-6">
          <h2 className="flex items-center gap-2 font-semibold text-amber-900">
            <AlertTriangle aria-hidden="true" className="size-5" />
            {t('users.permissionLookupTitle')}
          </h2>
          <p className="mt-2 text-sm leading-7 text-amber-900/90">
            {t('users.permissionLookupError')}
          </p>
          {mapped?.unmatched.length ? (
            <ul className="mt-3 list-inside list-disc text-sm text-amber-900">
              {mapped.unmatched.map((item) => (
                <li key={item}>{item}</li>
              ))}
            </ul>
          ) : null}
        </div>
        <BackLink />
      </section>
    )
  }

  const submit = (values: UserFormValues) => {
    setErrorMessage('')
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
          navigate(paths.user(userId))
        },
        onError: (error) => {
          setErrorMessage(describeApiError(error, t))
          toast.error(t('users.updateFailed'))
        },
      },
    )
  }

  return (
    <section className="space-y-5">
      <PageHeader
        breadcrumb={t('users.breadcrumb')}
        title={t('users.editTitle')}
        description={t('users.editSubtitle')}
      />
      <UserForm
        defaultValues={defaultValues}
        departments={lookupsQuery.data.departments}
        roles={lookupsQuery.data.roles}
        isPending={updateMutation.isPending}
        submitLabel={t('users.saveChanges')}
        errorMessage={errorMessage || undefined}
        onSubmit={submit}
        onCancel={() => navigate(paths.user(userId))}
      />
      <BackLink />
    </section>
  )
}

function BackLink() {
  const { t } = useTranslation()
  return (
    <Link to={paths.users} className="inline-flex text-sm font-medium text-primary hover:underline">
      {t('users.backToList')}
    </Link>
  )
}
