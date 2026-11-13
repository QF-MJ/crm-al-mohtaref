import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { PageHeader } from '@/components/common/PageHeader'
import { ErrorState } from '@/components/common/ErrorState'
import { paths } from '@/app/router/paths'
import { toApiError } from '@/api/errors'
import { UserForm } from '../components/UserForm'
import { useCreateUser, useUserLookups } from '../api/users.queries'
import type { UserFormValues } from '../schemas/user.schema'

const CREATE_DEFAULTS: UserFormValues = {
  accountName: '',
  fullName: '',
  email: '',
  isActive: true,
  password: '',
  permissions: [],
}

export function CreateUserPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const lookupsQuery = useUserLookups()
  const createMutation = useCreateUser()
  const [errorMessage, setErrorMessage] = useState('')

  const submit = (values: UserFormValues) => {
    setErrorMessage('')
    createMutation.mutate(
      {
        accountName: values.accountName,
        fullName: values.fullName,
        email: values.email,
        password: values.password ?? '',
        ...(values.permissions.length > 0 ? { permissions: values.permissions } : {}),
      },
      {
        onSuccess: (userId) => {
          toast.success(t('users.createSuccess'))
          if (userId) {
            navigate(paths.user(String(userId)), { replace: true })
          } else {
            navigate(paths.users, { replace: true })
          }
        },
        onError: (error) => {
          const { message } = toApiError(error)
          setErrorMessage(message || t('users.unknownError'))
          toast.error(t('users.createFailed'))
        },
      },
    )
  }

  if (lookupsQuery.isLoading) {
    return (
      <section className="space-y-5">
        <PageHeader breadcrumb={t('users.breadcrumb')} title={t('users.createTitle')} />
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

  if (lookupsQuery.isError || !lookupsQuery.data) {
    return (
      <section className="space-y-5">
        <PageHeader breadcrumb={t('users.breadcrumb')} title={t('users.createTitle')} />
        <div className="rounded-2xl border border-slate-200 bg-white">
          <ErrorState error={lookupsQuery.error} onRetry={() => void lookupsQuery.refetch()} />
        </div>
        <BackLink />
      </section>
    )
  }

  return (
    <section className="space-y-5">
      <PageHeader
        breadcrumb={t('users.createBreadcrumb')}
        title={t('users.createTitle')}
        description={t('users.createDescription')}
      />
      <UserForm
        defaultValues={CREATE_DEFAULTS}
        departments={lookupsQuery.data.departments}
        roles={lookupsQuery.data.roles}
        isPending={createMutation.isPending}
        createMode
        submitLabel={t('users.create')}
        errorMessage={errorMessage || undefined}
        onSubmit={submit}
        onCancel={() => navigate(paths.users)}
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
