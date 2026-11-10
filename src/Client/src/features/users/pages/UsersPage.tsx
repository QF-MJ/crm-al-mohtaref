import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ArrowLeft, ArrowRight, Search } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { paths } from '@/app/router/paths'
import { getApiErrorMessage } from '@/api/errors'
import { usePermission } from '@/permissions/usePermission'
import { useUserLookups, useUsers } from '../api/users.queries'
import type { UserFilters, UserListItem } from '../types'

export function UsersPage() {
  const { t } = useTranslation()
  const [filters, setFilters] = useState<UserFilters>({ pageNumber: 1 })
  const [accountName, setAccountName] = useState('')
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [status, setStatus] = useState('')
  const [roleId, setRoleId] = useState('')
  const [departmentId, setDepartmentId] = useState('')
  const usersQuery = useUsers(filters)
  const lookupsQuery = useUserLookups()
  const page = usersQuery.data?.page

  const applyFilters = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFilters({
      pageNumber: 1,
      accountName: accountName.trim() || undefined,
      name: name.trim() || undefined,
      email: email.trim() || undefined,
      isActive: status === '' ? undefined : status === 'true',
      roleId: roleId ? Number(roleId) : undefined,
      departmentId: departmentId ? Number(departmentId) : undefined,
    })
  }

  const clearFilters = () => {
    setAccountName('')
    setName('')
    setEmail('')
    setStatus('')
    setRoleId('')
    setDepartmentId('')
    setFilters({ pageNumber: 1 })
  }

  return (
    <section className="-m-6 min-h-full space-y-5 bg-slate-50 p-5 sm:p-6" dir="rtl">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="mb-1 text-xs text-slate-400">{t('users.breadcrumb')}</p>
          <h1 className="text-2xl font-bold text-slate-800">{t('users.title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('users.subtitle')}</p>
        </div>
      </header>

      <form
        onSubmit={applyFilters}
        aria-label={t('users.filters')}
        className="rounded-xl border bg-white p-4 sm:p-5"
      >
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-6">
          <FilterField
            label={t('users.username')}
            value={accountName}
            onChange={setAccountName}
          />
          <FilterField label={t('users.name')} value={name} onChange={setName} />
          <FilterField label={t('users.email')} value={email} onChange={setEmail} />
          <SelectField
            label={t('users.status')}
            value={status}
            onChange={setStatus}
            options={[
              { value: '', label: t('users.allStatuses') },
              { value: 'true', label: t('users.active') },
              { value: 'false', label: t('users.inactive') },
            ]}
          />
          <SelectField
            label={t('users.role')}
            value={roleId}
            onChange={setRoleId}
            options={[
              { value: '', label: t('users.allRoles') },
              ...(lookupsQuery.data?.roles ?? []).map((role) => ({
                value: String(role.id),
                label: role.name,
              })),
            ]}
          />
          <SelectField
            label={t('users.department')}
            value={departmentId}
            onChange={setDepartmentId}
            options={[
              { value: '', label: t('users.allDepartments') },
              ...(lookupsQuery.data?.departments ?? []).map((department) => ({
                value: String(department.id),
                label: department.name,
              })),
            ]}
          />
        </div>
        {lookupsQuery.isError && (
          <p className="mt-3 text-sm text-destructive" role="alert">
            {getApiErrorMessage(lookupsQuery.error, t('users.unknownError'))}
          </p>
        )}
        <div className="mt-4 flex justify-center gap-2">
          <Button type="submit" size="sm" className="bg-teal-700 text-white hover:bg-teal-800">
            <Search aria-hidden="true" />
            {t('users.search')}
          </Button>
          <Button type="button" variant="outline" size="sm" onClick={clearFilters}>
            {t('users.clearFilters')}
          </Button>
        </div>
      </form>

      <section className="overflow-hidden rounded-xl border bg-white">
        <div className="flex flex-wrap items-center justify-between gap-3 border-b px-4 py-4 sm:px-5">
          <div>
            <h2 className="font-semibold text-slate-800">{t('users.listTitle')}</h2>
            <p className="mt-1 text-xs text-slate-500">{t('users.listDescription')}</p>
          </div>
          {!usersQuery.isLoading && !usersQuery.isError && (
            <span className="text-xs text-slate-500">
              {t('users.pageCount', {
                count: usersQuery.data?.items.length ?? 0,
                page: page?.pageNumber ?? filters.pageNumber,
              })}
            </span>
          )}
        </div>

        {usersQuery.isLoading ? (
          <p className="p-8 text-center text-sm text-slate-500" role="status">
            {t('users.loading')}
          </p>
        ) : usersQuery.isError ? (
          <div className="space-y-3 p-8 text-center" role="alert">
            <p className="text-sm text-destructive">
              {getApiErrorMessage(usersQuery.error, t('users.unknownError'))}
            </p>
            <Button
              type="button"
              variant="outline"
              onClick={() => void usersQuery.refetch()}
              disabled={usersQuery.isFetching}
            >
              {t('users.retry')}
            </Button>
          </div>
        ) : usersQuery.data.items.length === 0 ? (
          <p className="p-8 text-center text-sm text-slate-500">{t('users.empty')}</p>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[700px] text-start text-sm">
                <thead className="bg-slate-50 text-xs font-medium text-slate-500">
                  <tr>
                    <th scope="col" className="px-4 py-3">{t('users.username')}</th>
                    <th scope="col" className="px-4 py-3">{t('users.name')}</th>
                    <th scope="col" className="px-4 py-3">{t('users.email')}</th>
                    <th scope="col" className="px-4 py-3">{t('users.status')}</th>
                    <th scope="col" className="px-4 py-3">{t('users.actions')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {usersQuery.data.items.map((user) => (
                    <UserRow key={user.id} user={user} />
                  ))}
                </tbody>
              </table>
            </div>
            <footer className="flex items-center justify-between border-t px-4 py-4 text-xs text-slate-500 sm:px-5">
              <span>
                {t('users.pageNumber', { page: page?.pageNumber ?? filters.pageNumber })}
              </span>
              <div className="flex gap-2">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  disabled={(page?.pageNumber ?? filters.pageNumber) <= 1 || usersQuery.isFetching}
                  onClick={() =>
                    setFilters((current) => ({ ...current, pageNumber: current.pageNumber - 1 }))
                  }
                >
                  <ArrowRight aria-hidden="true" />
                  {t('users.previousPage')}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  disabled={!page?.hasNextPage || usersQuery.isFetching}
                  onClick={() =>
                    setFilters((current) => ({ ...current, pageNumber: current.pageNumber + 1 }))
                  }
                >
                  {t('users.nextPage')}
                  <ArrowLeft aria-hidden="true" />
                </Button>
              </div>
            </footer>
          </>
        )}
      </section>
    </section>
  )
}

function FilterField({
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
      <Input
        className="text-sm"
        value={value}
        onChange={(event) => onChange(event.target.value)}
      />
    </label>
  )
}

function SelectField({
  label,
  value,
  onChange,
  options,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  options: { value: string; label: string }[]
}) {
  return (
    <label className="space-y-1.5 text-xs font-medium text-slate-500">
      <span>{label}</span>
      <select
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="h-9 w-full rounded-md border border-input bg-white px-3 text-sm text-slate-700 outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
      >
        {options.map((option) => (
          <option key={option.value || option.label} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
  )
}

function UserRow({ user }: { user: UserListItem }) {
  const { t } = useTranslation()
  const can = usePermission()
  return (
    <tr className="transition-colors hover:bg-teal-50/60">
      <td className="whitespace-nowrap px-4 py-3 font-medium text-slate-700" dir="auto">
        <Link to={paths.user(String(user.id))} className="hover:text-primary hover:underline">
          {user.accountName}
        </Link>
      </td>
      <td className="whitespace-nowrap px-4 py-3 text-slate-700" dir="auto">
        {user.fullName ?? '—'}
      </td>
      <td className="whitespace-nowrap px-4 py-3 text-slate-500" dir="ltr">
        {user.email ?? '—'}
      </td>
      <td className="whitespace-nowrap px-4 py-3">
        {user.isActive === undefined ? (
          <span className="text-slate-400">—</span>
        ) : (
          <span
            className={`inline-flex items-center gap-2 rounded-full px-3 py-1 text-xs font-medium ${
              user.isActive ? 'bg-teal-50 text-teal-800' : 'bg-slate-100 text-slate-500'
            }`}
          >
            <span
              className={`size-1.5 rounded-full ${user.isActive ? 'bg-teal-700' : 'bg-slate-400'}`}
            />
            {t(user.isActive ? 'users.active' : 'users.inactive')}
          </span>
        )}
      </td>
      <td className="whitespace-nowrap px-4 py-3">
        {can('users.suspend') && (
          <div className="flex gap-2">
            <Link to={paths.user(String(user.id))} className="inline-flex h-8 items-center rounded-md border bg-white px-3 text-sm font-medium hover:bg-accent">{t('users.view')}</Link>
            <Link to={paths.userEdit(String(user.id))} className="inline-flex h-8 items-center rounded-md border bg-white px-3 text-sm font-medium hover:bg-accent">{t('users.edit')}</Link>
          </div>
        )}
      </td>
    </tr>
  )
}
