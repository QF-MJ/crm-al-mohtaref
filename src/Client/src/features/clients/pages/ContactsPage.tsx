import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { toApiError } from '@/api/errors'
import { usePermission } from '@/permissions/usePermission'
import { InvalidContactsResponseError } from '../api/contacts.api'
import { useContacts, useContactLookups } from '../api/contacts.queries'
import type { ContactFilters } from '../types'

function errorText(
  error: unknown,
  fallback: string,
  connectionError: string,
  invalidResponse: string,
) {
  if (error instanceof InvalidContactsResponseError) return invalidResponse
  const apiError = toApiError(error)
  if (apiError.status === 0) return connectionError
  return apiError.message === 'Unknown error' ? fallback : apiError.message
}

export function ContactsPage() {
  const { t } = useTranslation()
  const can = usePermission()
  const canFilter = can('clients.filter')
  const [filters, setFilters] = useState<ContactFilters>({ pageNumber: 1 })
  const [form, setForm] = useState({
    campaignTitle: '',
    trackId: '',
    leadStatusId: '',
    tagIds: [] as number[],
    studentName: '',
    contactName: '',
    phoneNumber: '',
  })
  const contactsQuery = useContacts(filters)
  const lookupsQuery = useContactLookups()
  const page = contactsQuery.data?.page

  const applyFilters = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFilters({
      pageNumber: 1,
      campaignTitle: form.campaignTitle.trim() || undefined,
      trackId: form.trackId ? Number(form.trackId) : undefined,
      leadStatusId: form.leadStatusId ? Number(form.leadStatusId) : undefined,
      tagIds: form.tagIds.length ? form.tagIds : undefined,
      studentName: form.studentName.trim() || undefined,
      contactName: form.contactName.trim() || undefined,
      phoneNumber: form.phoneNumber.trim() || undefined,
    })
  }

  const clearFilters = () => {
    setForm({
      campaignTitle: '',
      trackId: '',
      leadStatusId: '',
      tagIds: [],
      studentName: '',
      contactName: '',
      phoneNumber: '',
    })
    setFilters({ pageNumber: 1 })
  }

  return (
    <section className="-m-4 min-h-full space-y-5 bg-slate-50 p-4 sm:-m-6 sm:p-6" dir="rtl">
      <header>
        <p className="mb-1 text-xs text-slate-400">{t('contacts.breadcrumb')}</p>
        <h1 className="text-2xl font-bold text-slate-800">{t('contacts.title')}</h1>
      </header>

      {canFilter && (
        <form onSubmit={applyFilters} className="space-y-4 rounded-xl border bg-white p-4 sm:p-5">
          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            <TextFilter
              label={t('contacts.contactName')}
              value={form.contactName}
              onChange={(contactName) => setForm((current) => ({ ...current, contactName }))}
            />
            <TextFilter
              label={t('contacts.studentName')}
              value={form.studentName}
              onChange={(studentName) => setForm((current) => ({ ...current, studentName }))}
            />
            <TextFilter
              label={t('contacts.phoneNumber')}
              value={form.phoneNumber}
              onChange={(phoneNumber) => setForm((current) => ({ ...current, phoneNumber }))}
            />
            <TextFilter
              label={t('contacts.campaignTitle')}
              value={form.campaignTitle}
              onChange={(campaignTitle) => setForm((current) => ({ ...current, campaignTitle }))}
            />
            <label className="space-y-1.5 text-xs font-medium text-slate-500">
              <span>{t('contacts.track')}</span>
              <select
                className="h-9 w-full rounded-md border bg-white px-3 text-sm"
                value={form.trackId}
                onChange={(event) =>
                  setForm((current) => ({ ...current, trackId: event.target.value }))
                }
              >
                <option value="">{t('contacts.all')}</option>
                {(lookupsQuery.data?.tracks ?? []).map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.name}
                  </option>
                ))}
              </select>
            </label>
            <label className="space-y-1.5 text-xs font-medium text-slate-500">
              <span>{t('contacts.leadStatus')}</span>
              <select
                className="h-9 w-full rounded-md border bg-white px-3 text-sm"
                value={form.leadStatusId}
                onChange={(event) =>
                  setForm((current) => ({ ...current, leadStatusId: event.target.value }))
                }
              >
                <option value="">{t('contacts.all')}</option>
                {(lookupsQuery.data?.leadStatuses ?? []).map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.name}
                  </option>
                ))}
              </select>
            </label>
            <fieldset className="space-y-2 text-xs font-medium text-slate-500 xl:col-span-2">
              <legend>{t('contacts.tags')}</legend>
              <div className="flex flex-wrap gap-3">
                {(lookupsQuery.data?.tags ?? []).map((tag) => (
                  <label key={tag.id} className="inline-flex items-center gap-2">
                    <input
                      type="checkbox"
                      checked={form.tagIds.includes(tag.id)}
                      onChange={(event) =>
                        setForm((current) => ({
                          ...current,
                          tagIds: event.target.checked
                            ? [...current.tagIds, tag.id]
                            : current.tagIds.filter((id) => id !== tag.id),
                        }))
                      }
                    />
                    {tag.tag}
                  </label>
                ))}
              </div>
              {lookupsQuery.data && lookupsQuery.data.tags === undefined && (
                <p className="text-sm text-amber-700" role="status">
                  {t('contacts.tagsUnavailable')}
                </p>
              )}
            </fieldset>
          </div>
          {lookupsQuery.isError && (
            <div className="flex flex-wrap items-center justify-between gap-3" role="alert">
              <p className="text-sm text-destructive">
                {errorText(
                  lookupsQuery.error,
                  t('contacts.error'),
                  t('contacts.connectionError'),
                  t('contacts.invalidResponse'),
                )}
              </p>
              <Button
                type="button"
                variant="outline"
                onClick={() => void lookupsQuery.refetch()}
                disabled={lookupsQuery.isFetching}
              >
                {t('contacts.retry')}
              </Button>
            </div>
          )}
          <div className="flex justify-center gap-2">
            <Button type="submit">{t('contacts.search')}</Button>
            <Button type="button" variant="outline" onClick={clearFilters}>
              {t('contacts.clear')}
            </Button>
          </div>
        </form>
      )}

      <section className="overflow-hidden rounded-xl border bg-white">
        {contactsQuery.isLoading ? (
          <p className="p-8 text-center text-sm text-slate-500" role="status">
            {t('contacts.loading')}
          </p>
        ) : contactsQuery.isError ? (
          <div className="space-y-3 p-8 text-center" role="alert">
            <p className="text-sm text-destructive">
              {errorText(
                contactsQuery.error,
                t('contacts.error'),
                t('contacts.connectionError'),
                t('contacts.invalidResponse'),
              )}
            </p>
            <Button
              type="button"
              variant="outline"
              onClick={() => void contactsQuery.refetch()}
              disabled={contactsQuery.isFetching}
            >
              {t('contacts.retry')}
            </Button>
          </div>
        ) : !contactsQuery.data ? (
          <div className="space-y-3 p-8 text-center" role="alert">
            <p className="text-sm text-destructive">{t('contacts.invalidResponse')}</p>
            <Button
              type="button"
              variant="outline"
              onClick={() => void contactsQuery.refetch()}
              disabled={contactsQuery.isFetching}
            >
              {t('contacts.retry')}
            </Button>
          </div>
        ) : !contactsQuery.data.items.length ? (
          <p className="p-8 text-center text-sm text-slate-500">{t('contacts.empty')}</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[560px] text-start text-sm">
              <thead className="bg-slate-50 text-xs font-medium text-slate-500">
                <tr>
                  <th className="px-4 py-3">{t('contacts.contactName')}</th>
                  <th className="px-4 py-3">{t('contacts.students')}</th>
                  <th className="px-4 py-3">{t('contacts.id')}</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {contactsQuery.data.items.map((contact) => (
                  <tr key={contact.id}>
                    <td className="px-4 py-3 font-medium">{contact.name}</td>
                    <td className="px-4 py-3">
                      {contact.students?.map((student) => student.name).join('، ') || '—'}
                    </td>
                    <td className="px-4 py-3 text-slate-500">{contact.id}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        {contactsQuery.data && (
          <footer className="flex items-center justify-between border-t px-4 py-3 text-xs text-slate-500">
            <span>{t('contacts.page', { page: page?.pageNumber ?? filters.pageNumber })}</span>
            <div className="flex gap-2">
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={(page?.pageNumber ?? 1) <= 1 || contactsQuery.isFetching}
                onClick={() =>
                  setFilters((current) => ({
                    ...current,
                    pageNumber: Math.max(1, current.pageNumber - 1),
                  }))
                }
              >
                {t('contacts.previous')}
              </Button>
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={!page?.hasNextPage || contactsQuery.isFetching}
                onClick={() =>
                  setFilters((current) => ({
                    ...current,
                    pageNumber: current.pageNumber + 1,
                  }))
                }
              >
                {t('contacts.next')}
              </Button>
            </div>
          </footer>
        )}
      </section>
    </section>
  )
}

function TextFilter({
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
