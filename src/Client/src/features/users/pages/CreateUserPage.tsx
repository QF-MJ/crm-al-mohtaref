import { Link, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { paths } from '@/app/router/paths'
import { getApiErrorMessage } from '@/api/errors'
import { UserForm } from '../components/UserForm'
import { useCreateUser, useUserLookups } from '../api/users.queries'
import type { UserFormValues } from '../schemas/user.schema'

export function CreateUserPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const lookupsQuery = useUserLookups()
  const createMutation = useCreateUser()

  const submit = (values: UserFormValues) => {
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
          navigate(paths.user(String(userId)), { replace: true })
        },
        onError: (error) => toast.error(getApiErrorMessage(error, t('users.unknownError'))),
      },
    )
  }

  if (lookupsQuery.isLoading) {
    return <p className="p-8 text-center text-sm text-slate-500">{t('users.loading')}</p>
  }

  if (lookupsQuery.isError || !lookupsQuery.data) {
    return (
      <section className="mx-auto max-w-3xl space-y-4 rounded-xl border bg-white p-6 text-center" role="alert">
        <p className="text-sm text-destructive">
          {getApiErrorMessage(lookupsQuery.error, t('users.unknownError'))}
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

  return (
    <section className="mx-auto max-w-3xl space-y-5" dir="rtl">
      <header>
        <p className="mb-1 text-xs text-slate-400">{t('users.createBreadcrumb')}</p>
        <h1 className="text-2xl font-bold text-slate-800">{t('users.createTitle')}</h1>
        <p className="mt-1 text-sm text-slate-500">{t('users.createDescription')}</p>
      </header>
      <UserForm
        defaultValues={{
          accountName: '',
          fullName: '',
          email: '',
          password: '',
          isActive: true,
          permissions: [],
        }}
        departments={lookupsQuery.data.departments}
        roles={lookupsQuery.data.roles}
        isPending={createMutation.isPending}
        createMode
        showStatus={false}
        showPassword
        submitLabel={t('users.create')}
        onSubmit={submit}
      />
      <Link to={paths.users} className="inline-flex text-sm font-medium text-primary hover:underline">
        {t('users.backToList')}
      </Link>
    </section>
  )
}
