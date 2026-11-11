import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { toApiError } from '@/api/errors'
import { usePermission } from '@/permissions/usePermission'
import { InvalidContactsResponseError } from '../api/contacts.api'
import { useContacts } from '../api/contacts.queries'
import type { Contact, ContactFilters } from '../types'

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

function parsePositiveInteger(value: string): number | undefined | null {
  if (!value.trim()) return undefined
  const parsed = Number(value)
  return Number.isInteger(parsed) && parsed > 0 ? parsed : null
}

function parseTagIds(value: string): number[] | undefined | null {
  if (!value.trim()) return undefined
  const ids: number[] = []
  for (const part of value.split(',')) {
    const id = parsePositiveInteger(part.trim())
    if (id === null || id === undefined) return null
    ids.push(id)
  }
  return ids
}

function isRecord(value: unknown): value is Contact {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function getFieldValue(record: Contact, fieldName: string): unknown {
  const entry = Object.entries(record).find(
    ([field]) => field.toLowerCase() === fieldName.toLowerCase(),
  )
  return entry?.[1]
}

function getContactName(contact: Contact): string | undefined {
  for (const field of ['name', 'contactName']) {
    const value = getFieldValue(contact, field)
    if (typeof value === 'string' && value.trim()) return value
  }
  return undefined
}

function getStudentNames(contact: Contact): string[] | undefined {
  const students = getFieldValue(contact, 'students')
  if (!Array.isArray(students)) return undefined

  return students.flatMap((student) => {
    if (typeof student === 'string' && student.trim()) return [student]
    if (!isRecord(student)) return []

    const name = getContactName(student)
    return name ? [name] : []
  })
}

export function ContactsPage() {
  const { t } = useTranslation()
  const can = usePermission()
  const canFilter = can('clients.filter')
  const [filters, setFilters] = useState<ContactFilters>({ pageNumber: 1 })
  const [filterError, setFilterError] = useState('')
  const [form, setForm] = useState({
    campaignTitle: '',
    trackId: '',
    leadStatusId: '',
    tagIds: '',
    studentName: '',
    contactName: '',
    phoneNumber: '',
  })
  const contactsQuery = useContacts(filters)
  const page = contactsQuery.data?.page

  const applyFilters = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const trackId = parsePositiveInteger(form.trackId)
    const leadStatusId = parsePositiveInteger(form.leadStatusId)
    const tagIds = parseTagIds(form.tagIds)
    if (trackId === null || leadStatusId === null || tagIds === null) {
      setFilterError(t('contacts.invalidFilter'))
      return
    }

    setFilterError('')
    setFilters({
      pageNumber: 1,
      campaignTitle: form.campaignTitle.trim() || undefined,
      trackId,
      leadStatusId,
      tagIds,
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
      tagIds: '',
      studentName: '',
      contactName: '',
      phoneNumber: '',
    })
    setFilterError('')
    setFilters({ pageNumber: 1 })
  }

  return (
    <section className="-m-4 min-h-full space-y-5 bg-slate-50 p-4 sm:-m-6 sm:p-6" dir="rtl">
      <header>
        <p className="mb-1 text-xs text-slate-400">{t('contacts.breadcrumb')}</p>
        <h1 className="text-2xl font-bold text-slate-800">{t('contacts.title')}</h1>
      </header>

      {canFilter && (
        <form
          noValidate
          onSubmit={applyFilters}
          className="space-y-4 rounded-xl border bg-white p-4 sm:p-5"
        >
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
            <NumberFilter
              label={t('contacts.trackId')}
              value={form.trackId}
              onChange={(trackId) => setForm((current) => ({ ...current, trackId }))}
            />
            <NumberFilter
              label={t('contacts.leadStatusId')}
              value={form.leadStatusId}
              onChange={(leadStatusId) => setForm((current) => ({ ...current, leadStatusId }))}
            />
            <TextFilter
              label={t('contacts.tagIds')}
              value={form.tagIds}
              placeholder="1, 2"
              onChange={(tagIds) => setForm((current) => ({ ...current, tagIds }))}
            />
          </div>
          <p className="text-xs leading-5 text-amber-700">{t('contacts.lookupsUnverified')}</p>
          {filterError && (
            <p className="text-sm text-destructive" role="alert">
              {filterError}
            </p>
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
        <header className="border-b px-4 py-4 sm:px-5">
          <h2 className="font-semibold text-slate-800">{t('contacts.results')}</h2>
          {contactsQuery.data && (
            <p className="mt-2 text-xs text-slate-500">
              {t('contacts.currentPageCount', { count: contactsQuery.data.items.length })}
            </p>
          )}
        </header>
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
          <p className="p-8 text-center text-sm text-destructive" role="alert">
            {t('contacts.invalidResponse')}
          </p>
        ) : !contactsQuery.data.items.length ? (
          <p className="p-8 text-center text-sm text-slate-500">{t('contacts.empty')}</p>
        ) : (
          <div className="grid gap-4 p-4 sm:grid-cols-2 xl:grid-cols-3">
            {contactsQuery.data.items.map((contact, index) => (
              <ContactCard key={index} contact={contact} />
            ))}
          </div>
        )}
        {page && (
          <footer className="flex justify-between gap-2 border-t px-4 py-3">
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={page.pageNumber <= 1 || contactsQuery.isFetching}
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
              disabled={!page.hasNextPage || contactsQuery.isFetching}
              onClick={() =>
                setFilters((current) => ({
                  ...current,
                  pageNumber: current.pageNumber + 1,
                }))
              }
            >
              {t('contacts.next')}
            </Button>
          </footer>
        )}
      </section>
    </section>
  )
}

function ContactCard({ contact }: { contact: Contact }) {
  const { t } = useTranslation()
  const name = getContactName(contact)
  const studentNames = getStudentNames(contact)

  return (
    <article className="min-w-0 space-y-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
      <header className="border-b border-slate-100 pb-3">
        <h3 className="truncate text-lg font-semibold text-slate-800" title={name}>
          {name ?? t('contacts.unnamedContact')}
        </h3>
      </header>
      {studentNames !== undefined && (
        <section className="space-y-2">
          <div className="flex items-center justify-between gap-2">
            <h4 className="text-sm font-medium text-slate-600">{t('contacts.students')}</h4>
            <span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs text-slate-600">
              {t('contacts.studentsCount', { count: studentNames.length })}
            </span>
          </div>
          {studentNames.length > 0 ? (
            <ul className="flex flex-wrap gap-2">
              {studentNames.map((studentName, index) => (
                <li
                  key={`${studentName}-${index}`}
                  className="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-700"
                >
                  {studentName}
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-sm text-slate-400">{t('contacts.noStudentNames')}</p>
          )}
        </section>
      )}
    </article>
  )
}

function TextFilter({
  label,
  value,
  onChange,
  placeholder,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  placeholder?: string
}) {
  return (
    <label className="space-y-1.5 text-xs font-medium text-slate-500">
      <span>{label}</span>
      <Input
        value={value}
        placeholder={placeholder}
        onChange={(event) => onChange(event.target.value)}
      />
    </label>
  )
}

function NumberFilter({
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
        type="number"
        min={1}
        step={1}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      />
    </label>
  )
}
