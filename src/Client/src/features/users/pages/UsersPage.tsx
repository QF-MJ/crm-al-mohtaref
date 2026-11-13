import { useMemo, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Search, ShieldCheck, UserPlus, UsersRound } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Select, type SelectOption } from '@/components/ui/select'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { TableSkeleton } from '@/components/common/TableSkeleton'
import { Pagination } from '@/components/common/Pagination'
import { paths } from '@/app/router/paths'
import { useAccess } from '@/permissions/useAccess'
import { useUserLookups, useUsers } from '../api/users.queries'
import type { UserFilters, UserListItem } from '../types'

interface DraftFilters {
  name: string
  accountName: string
  email: string
  status: string
  roleId: string
  departmentId: string
}

const EMPTY_DRAFT: DraftFilters = {
  name: '',
  accountName: '',
  email: '',
  status: '',
  roleId: '',
  departmentId: '',
}

export function UsersPage() {
  const { t } = useTranslation()
  const access = useAccess()
  const [draft, setDraft] = useState<DraftFilters>(EMPTY_DRAFT)
  const [filters, setFilters] = useState<UserFilters>({ pageNumber: 1 })

  const lookupsQuery = useUserLookups()
  const departments = lookupsQuery.data?.departments
  const roles = lookupsQuery.data?.roles ?? []

  // مدير الإدارة يرى كل الأقسام، وغيره يرى أقسامه فقط (الباك يفرض ذلك)
  const departmentOptions = useMemo(() => {
    const all = departments ?? []
    return access.isAdmin
      ? all
      : all.filter((department) => access.managerDepartments.includes(department.name.trim()))
  }, [departments, access.isAdmin, access.managerDepartments])

  const defaultDepartmentId =
    !access.isAdmin && departmentOptions[0] ? String(departmentOptions[0].id) : undefined

  // مدير القسم يجب أن يرسل departmentId، وإلا يرفض الباك الطلب:
  // نبنيه أثناء العرض بدل تعديل الحالة داخل effect
  const effectiveDepartmentId = access.isAdmin
    ? filters.departmentId
    : filters.departmentId || defaultDepartmentId

  const effectiveFilters = useMemo<UserFilters>(
    () => ({ ...filters, departmentId: effectiveDepartmentId }),
    [filters, effectiveDepartmentId],
  )

  const canQuery = access.isAdmin || Boolean(effectiveDepartmentId)
  const usersQuery = useUsers(effectiveFilters, canQuery)

  const roleOptions: SelectOption[] = [
    { value: '', label: t('users.allRoles') },
    ...roles.map((role) => ({ value: String(role.id), label: role.name })),
  ]

  const departmentFilterOptions: SelectOption[] = [
    ...(access.isAdmin ? [{ value: '', label: t('users.allDepartments') }] : []),
    ...departmentOptions.map((department) => ({
      value: String(department.id),
      label: department.name,
    })),
  ]

  const applyFilters = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const departmentId = access.isAdmin
      ? draft.departmentId || undefined
      : draft.departmentId || defaultDepartmentId

    setFilters({
      pageNumber: 1,
      name: draft.name.trim() || undefined,
      accountName: draft.accountName.trim() || undefined,
      email: draft.email.trim() || undefined,
      isActive: draft.status === '' ? undefined : draft.status === 'true',
      roleId: draft.roleId || undefined,
      departmentId,
    })
  }

  const clearFilters = () => {
    setDraft(EMPTY_DRAFT)
    setFilters({ pageNumber: 1 })
  }

  const hasFilters = Boolean(
    filters.name ||
    filters.accountName ||
    filters.email ||
    filters.roleId ||
    filters.isActive !== undefined,
  )
  const pageNumber = filters.pageNumber
  const page = usersQuery.data?.page

  return (
    <section className="space-y-5">
      <PageHeader
        breadcrumb={t('users.breadcrumb')}
        title={t('users.title')}
        description={
          access.isAdmin
            ? t('users.subtitleAll')
            : t('users.subtitleScope', { department: access.managerDepartments.join('، ') })
        }
        actions={
          access.can('users.create') ? (
            <Link
              to={paths.usersNew}
              className="inline-flex h-9 items-center gap-2 rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground hover:bg-primary/90"
            >
              <UserPlus aria-hidden="true" className="size-4" />
              {t('users.add')}
            </Link>
          ) : null
        }
      />

      {!access.isAdmin && (
        <div className="flex items-start gap-3 rounded-xl border border-sky-200 bg-sky-50 px-4 py-3 text-sm text-sky-900">
          <ShieldCheck aria-hidden="true" className="mt-0.5 size-[18px] shrink-0" />
          <p className="leading-6">{t('users.scopeNote')}</p>
        </div>
      )}

      <form
        onSubmit={applyFilters}
        aria-label={t('users.filters')}
        className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"
      >
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          <TextField
            label={t('users.fullName')}
            value={draft.name}
            onChange={(name) => setDraft((current) => ({ ...current, name }))}
          />
          <TextField
            label={t('users.username')}
            value={draft.accountName}
            onChange={(accountName) => setDraft((current) => ({ ...current, accountName }))}
          />
          <TextField
            label={t('users.email')}
            value={draft.email}
            onChange={(email) => setDraft((current) => ({ ...current, email }))}
          />
          <SelectField
            label={t('users.status')}
            value={draft.status}
            onChange={(status) => setDraft((current) => ({ ...current, status }))}
            options={[
              { value: '', label: t('users.allStatuses') },
              { value: 'true', label: t('users.active') },
              { value: 'false', label: t('users.inactive') },
            ]}
          />
          <SelectField
            label={t('users.role')}
            value={draft.roleId}
            onChange={(roleId) => setDraft((current) => ({ ...current, roleId }))}
            options={roleOptions}
          />
          <SelectField
            label={t('users.department')}
            value={draft.departmentId || defaultDepartmentId || ''}
            onChange={(departmentId) => setDraft((current) => ({ ...current, departmentId }))}
            options={departmentFilterOptions}
            disabled={departmentFilterOptions.length <= 1}
          />
        </div>

        {lookupsQuery.isError && (
          <p role="alert" className="mt-3 text-sm text-destructive">
            {t('users.lookupsError')}
          </p>
        )}

        <div className="mt-4 flex flex-wrap justify-center gap-2">
          <Button type="submit" size="sm" disabled={!canQuery}>
            <Search aria-hidden="true" />
            {t('actions.search')}
          </Button>
          <Button type="button" variant="outline" size="sm" onClick={clearFilters}>
            {t('users.clearFilters')}
          </Button>
        </div>
      </form>

      <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
        <header className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 px-4 py-4 sm:px-5">
          <div>
            <h2 className="font-semibold text-slate-900">{t('users.listTitle')}</h2>
            <p className="mt-1 text-xs leading-6 text-slate-500">{t('users.listDescription')}</p>
          </div>
          {usersQuery.data && (
            <Badge variant="neutral">
              {t('users.pageCount', { count: usersQuery.data.items.length })}
            </Badge>
          )}
        </header>

        {usersQuery.isLoading ? (
          <TableSkeleton rows={6} columns={5} />
        ) : usersQuery.isError ? (
          <ErrorState
            error={usersQuery.error}
            onRetry={() => void usersQuery.refetch()}
            isRetrying={usersQuery.isFetching}
          />
        ) : !canQuery ? (
          <EmptyState
            icon={ShieldCheck}
            title={t('users.scopeUnavailableTitle')}
            description={t('users.scopeUnavailableDescription')}
          />
        ) : !usersQuery.data?.items.length ? (
          <EmptyState
            icon={UsersRound}
            title={hasFilters ? t('users.emptyFiltered') : t('users.empty')}
            description={hasFilters ? t('users.emptyFilteredHint') : t('users.emptyHint')}
            action={
              pageNumber > 1 || hasFilters ? (
                <div className="flex flex-wrap justify-center gap-2">
                  {pageNumber > 1 && (
                    <Button
                      type="button"
                      size="sm"
                      onClick={() =>
                        setFilters((current) => ({
                          ...current,
                          pageNumber: Math.max(1, current.pageNumber - 1),
                        }))
                      }
                    >
                      {t('pagination.previous')}
                    </Button>
                  )}
                  {hasFilters && (
                    <Button type="button" variant="outline" size="sm" onClick={clearFilters}>
                      {t('users.clearFilters')}
                    </Button>
                  )}
                </div>
              ) : null
            }
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[720px] text-start text-sm">
                <thead className="bg-slate-50 text-xs font-medium text-slate-500">
                  <tr>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('users.fullName')}
                    </th>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('users.username')}
                    </th>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('users.email')}
                    </th>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('users.status')}
                    </th>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('users.actions')}
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {usersQuery.data.items.map((user) => (
                    <UserRow key={user.id} user={user} canEdit={access.can('users.update')} />
                  ))}
                </tbody>
              </table>
            </div>

            <Pagination
              pageNumber={pageNumber}
              hasNextPage={Boolean(page?.hasNextPage)}
              isFetching={usersQuery.isFetching}
              onPrevious={() =>
                setFilters((current) => ({
                  ...current,
                  pageNumber: Math.max(1, current.pageNumber - 1),
                }))
              }
              onNext={() =>
                setFilters((current) => ({ ...current, pageNumber: current.pageNumber + 1 }))
              }
            />
          </>
        )}
      </section>
    </section>
  )
}

function UserRow({ user, canEdit }: { user: UserListItem; canEdit: boolean }) {
  const { t } = useTranslation()

  return (
    <tr className="transition-colors hover:bg-teal-50/50">
      <td className="px-4 py-3 font-medium text-slate-800">
        <Link
          to={paths.user(user.id)}
          className="hover:text-primary hover:underline"
          dir="auto"
          title={user.fullName}
        >
          {user.fullName?.trim() || '—'}
        </Link>
      </td>
      <td className="px-4 py-3 text-slate-600" dir="ltr">
        {user.accountName}
      </td>
      <td className="px-4 py-3 text-slate-500" dir="ltr">
        {user.email || '—'}
      </td>
      <td className="px-4 py-3">
        {user.isActive ? (
          <Badge variant="success" dot>
            {t('users.active')}
          </Badge>
        ) : (
          <Badge variant="danger" dot>
            {t('users.inactive')}
          </Badge>
        )}
      </td>
      <td className="px-4 py-3">
        <div className="flex flex-wrap gap-2">
          <Link
            to={paths.user(user.id)}
            className="inline-flex h-8 items-center rounded-lg border border-slate-200 bg-white px-3 text-xs font-medium text-slate-700 hover:bg-slate-50"
          >
            {t('actions.view')}
          </Link>
          {canEdit && (
            <Link
              to={paths.userEdit(user.id)}
              className="inline-flex h-8 items-center rounded-lg border border-teal-200 bg-teal-50 px-3 text-xs font-medium text-teal-800 hover:bg-teal-100"
            >
              {t('actions.edit')}
            </Link>
          )}
        </div>
      </td>
    </tr>
  )
}

function TextField({
  label,
  value,
  onChange,
}: {
  label: string
  value: string
  onChange: (value: string) => void
}) {
  return (
    <label className="space-y-1.5 text-xs font-medium text-slate-500">
      <span>{label}</span>
      <Input value={value} onChange={(event) => onChange(event.target.value)} />
    </label>
  )
}

function SelectField({
  label,
  value,
  onChange,
  options,
  disabled = false,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  options: SelectOption[]
  disabled?: boolean
}) {
  return (
    <label className="space-y-1.5 text-xs font-medium text-slate-500">
      <span>{label}</span>
      <Select value={value} onValueChange={onChange} options={options} disabled={disabled} />
    </label>
  )
}
