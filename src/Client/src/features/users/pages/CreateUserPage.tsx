import { useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { paths } from '@/app/router/paths'
import { useCreateUser, useUserLookups } from '../api/users.queries'
import { UserForm } from '../components/UserForm'
import type { UserFormValues } from '../schemas/user.schema'
import type { UserCreateInput } from '../types'

function getErrorMessage(error: unknown) {
  if (typeof error === 'object' && error !== null && 'message' in error) {
    return String(error.message)
  }
  return ''
}

export function CreateUserPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const lookupsQuery = useUserLookups()
  const createMutation = useCreateUser()
  const defaultValues = useMemo<UserFormValues | undefined>(() => {
    const firstRole = lookupsQuery.data?.roles[0]
    const firstDepartment = lookupsQuery.data?.departments[0]
    if (!firstRole || !firstDepartment) return undefined

    return {
      accountName: '',
      fullName: '',
      email: '',
      isActive: true,
      permissions: [{ roleId: firstRole.id, departmentId: firstDepartment.id }],
    }
  }, [lookupsQuery.data])

  const submit = (values: UserFormValues) => {
    if (values.permissions.length === 0) {
      toast.error(t('users.permissionRequired'))
      return
    }
    if (!values.password?.trim()) {
      toast.error(t('users.required'))
      return
    }

    const body: UserCreateInput = {
      accountName: values.accountName,
      fullName: values.fullName,
      email: values.email,
      password: values.password,
      permissions: values.permissions,
    }
    createMutation.mutate(body, {
      onSuccess: () => {
        toast.success(t('users.createSuccess'))
        navigate(paths.users)
      },
      onError: (error) => toast.error(getErrorMessage(error) || t('users.unknownError')),
    })
  }

  if (lookupsQuery.isLoading) {
    return <p className="p-8 text-center text-sm text-slate-500">{t('users.loading')}</p>
  }

  if (lookupsQuery.isError) {
    return (
      <section
        className="mx-auto max-w-3xl space-y-4 rounded-xl border bg-white p-6 text-center"
        role="alert"
      >
        <p className="text-sm text-destructive">
          {getErrorMessage(lookupsQuery.error) || t('users.unknownError')}
        </p>
        <Button
          type="button"
          variant="outline"
          onClick={() => void lookupsQuery.refetch()}
          disabled={lookupsQuery.isFetching}
        >
          {t('users.retry')}
        </Button>
      </section>
    )
  }

  if (!defaultValues) {
    return (
      <section className="mx-auto max-w-3xl rounded-xl border bg-white p-6 text-center" role="alert">
        <p className="text-sm text-destructive">{t('users.lookupsEmpty')}</p>
      </section>
    )
  }

  return (
    <section className="-m-6 min-h-full space-y-5 bg-slate-50 p-5 sm:p-6" dir="rtl">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="mb-1 text-xs text-slate-400">{t('users.createBreadcrumb')}</p>
          <h1 className="text-2xl font-bold text-slate-800">{t('users.createTitle')}</h1>
        </div>
        <span className="rounded-full border px-4 py-2 text-xs text-slate-500">
          {t('users.unsaved')}
        </span>
      </header>

      <UserForm
        defaultValues={defaultValues}
        departments={lookupsQuery.data.departments}
        roles={lookupsQuery.data.roles}
        isPending={createMutation.isPending}
        showPassword
        showStatus={false}
        submitLabel={t('users.create')}
        accountFieldsClassName="sm:grid-cols-2 lg:grid-cols-3"
        onSubmit={submit}
      />
    </section>
  )
}
