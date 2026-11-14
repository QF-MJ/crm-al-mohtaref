import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Info, Search, UsersRound } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Select, type SelectOption } from '@/components/ui/select'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { TableSkeleton } from '@/components/common/TableSkeleton'
import { Pagination } from '@/components/common/Pagination'
import { useAccess } from '@/permissions/useAccess'
import { useLookups } from '@/features/lookups'
import { useContacts } from '../api/contacts.queries'
import type { Contact, ContactFilters } from '../types'

const CONTACT_LOOKUPS = ['Tracks', 'LeadStatuses'] as const

interface DraftFilters {
  contactName: string
  studentName: string
  phoneNumber: string
  campaignTitle: string
  trackId: string
  leadStatusId: string
}

const EMPTY_DRAFT: DraftFilters = {
  contactName: '',
  studentName: '',
  phoneNumber: '',
  campaignTitle: '',
  trackId: '',
  leadStatusId: '',
}

export function ContactsPage() {
  const { t } = useTranslation()
  const access = useAccess()
  const canFilter = access.can('contacts.filter')

  const [draft, setDraft] = useState<DraftFilters>(EMPTY_DRAFT)
  const [filters, setFilters] = useState<ContactFilters>({ pageNumber: 1 })
  const [selected, setSelected] = useState<Set<string>>(new Set())

  const lookupsQuery = useLookups([...CONTACT_LOOKUPS])
  const contactsQuery = useContacts(filters)

  const pageItems = contactsQuery.data?.items ?? []
  const pageIds = pageItems.map((contact) => contact.id)
  const selectedOnPage = pageIds.filter((id) => selected.has(id)).length
  const allOnPageSelected = pageIds.length > 0 && selectedOnPage === pageIds.length
  const someOnPageSelected = selectedOnPage > 0 && !allOnPageSelected

  const toggleOne = (id: string) => {
    setSelected((current) => {
      const next = new Set(current)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  // تحديد/إلغاء تحديد كل المعروضين في الصفحة الحالية
  const toggleAllOnPage = () => {
    setSelected((current) => {
      const next = new Set(current)
      if (allOnPageSelected) pageIds.forEach((id) => next.delete(id))
      else pageIds.forEach((id) => next.add(id))
      return next
    })
  }

  const clearSelection = () => setSelected(new Set())

  const trackOptions: SelectOption[] = [
    { value: '', label: t('contacts.allTracks') },
    ...(lookupsQuery.data?.tracks ?? []).map((track) => ({
      value: String(track.id),
      label: track.name,
    })),
  ]

  const leadStatusOptions: SelectOption[] = [
    { value: '', label: t('contacts.allLeadStatuses') },
    ...(lookupsQuery.data?.leadStatuses ?? []).map((status) => ({
      value: String(status.id),
      label: status.name,
    })),
  ]

  const applyFilters = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSelected(new Set())
    setFilters({
      pageNumber: 1,
      contactName: draft.contactName.trim() || undefined,
      studentName: draft.studentName.trim() || undefined,
      phoneNumber: draft.phoneNumber.trim() || undefined,
      campaignTitle: draft.campaignTitle.trim() || undefined,
      trackId: draft.trackId || undefined,
      leadStatusId: draft.leadStatusId || undefined,
    })
  }

  const clearFilters = () => {
    setDraft(EMPTY_DRAFT)
    setSelected(new Set())
    setFilters({ pageNumber: 1 })
  }

  const hasFilters = Boolean(
    filters.contactName ||
    filters.studentName ||
    filters.phoneNumber ||
    filters.campaignTitle ||
    filters.trackId ||
    filters.leadStatusId,
  )
  const pageNumber = filters.pageNumber
  const page = contactsQuery.data?.page

  return (
    <section className="space-y-5">
      <PageHeader
        breadcrumb={t('contacts.breadcrumb')}
        title={t('contacts.title')}
        description={t('contacts.subtitle')}
      />

      <div className="flex items-start gap-3 rounded-xl border border-sky-200 bg-sky-50 px-4 py-3 text-sm text-sky-900">
        <Info aria-hidden="true" className="mt-0.5 size-[18px] shrink-0" />
        <p className="leading-6">{t('contacts.dataScopeNote')}</p>
      </div>

      {canFilter && (
        <form
          noValidate
          onSubmit={applyFilters}
          aria-label={t('contacts.filters')}
          className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"
        >
          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
            <TextField
              label={t('contacts.contactName')}
              value={draft.contactName}
              onChange={(contactName) => setDraft((current) => ({ ...current, contactName }))}
            />
            <TextField
              label={t('contacts.studentName')}
              value={draft.studentName}
              onChange={(studentName) => setDraft((current) => ({ ...current, studentName }))}
            />
            <TextField
              label={t('contacts.phoneNumber')}
              value={draft.phoneNumber}
              onChange={(phoneNumber) => setDraft((current) => ({ ...current, phoneNumber }))}
              dir="ltr"
            />
            <TextField
              label={t('contacts.campaignTitle')}
              value={draft.campaignTitle}
              onChange={(campaignTitle) => setDraft((current) => ({ ...current, campaignTitle }))}
            />
            <SelectField
              label={t('contacts.track')}
              value={draft.trackId}
              options={trackOptions}
              disabled={lookupsQuery.isLoading || lookupsQuery.isError}
              onChange={(trackId) => setDraft((current) => ({ ...current, trackId }))}
            />
            <SelectField
              label={t('contacts.leadStatus')}
              value={draft.leadStatusId}
              options={leadStatusOptions}
              disabled={lookupsQuery.isLoading || lookupsQuery.isError}
              onChange={(leadStatusId) => setDraft((current) => ({ ...current, leadStatusId }))}
            />
          </div>

          {lookupsQuery.isError && (
            <p role="alert" className="mt-3 text-sm text-destructive">
              {t('contacts.lookupsError')}
            </p>
          )}

          <div className="mt-4 flex flex-wrap justify-center gap-2">
            <Button type="submit" size="sm">
              <Search aria-hidden="true" />
              {t('actions.search')}
            </Button>
            <Button type="button" variant="outline" size="sm" onClick={clearFilters}>
              {t('contacts.clearFilters')}
            </Button>
          </div>
        </form>
      )}

      <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
        <header className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 px-4 py-4 sm:px-5">
          <div>
            <h2 className="font-semibold text-slate-900">{t('contacts.results')}</h2>
            <p className="mt-1 text-xs leading-6 text-slate-500">
              {t('contacts.resultsDescription')}
            </p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            {pageItems.length > 0 && (
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={toggleAllOnPage}
                className="text-teal-800 hover:bg-teal-50"
              >
                {allOnPageSelected ? t('contacts.unselectAllPage') : t('contacts.selectAllPage')}
              </Button>
            )}
            {contactsQuery.data && (
              <Badge variant="neutral">
                {t('contacts.pageCount', { count: contactsQuery.data.items.length })}
              </Badge>
            )}
          </div>
        </header>

        {selected.size > 0 && (
          <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 bg-teal-50/50 px-4 py-3 sm:px-5">
            <p className="text-sm font-medium text-teal-900">
              {t('contacts.selectedCount', { count: selected.size })}
            </p>
            <div className="flex flex-wrap items-center gap-3">
              <span className="hidden text-xs text-teal-800/80 sm:block">
                {t('contacts.selectionHint')}
              </span>
              <Button type="button" variant="outline" size="sm" onClick={clearSelection}>
                {t('contacts.clearSelection')}
              </Button>
            </div>
          </div>
        )}

        {contactsQuery.isLoading ? (
          <TableSkeleton rows={6} columns={4} />
        ) : contactsQuery.isError ? (
          <ErrorState
            error={contactsQuery.error}
            onRetry={() => void contactsQuery.refetch()}
            isRetrying={contactsQuery.isFetching}
          />
        ) : !contactsQuery.data?.items.length ? (
          <EmptyState
            icon={UsersRound}
            title={hasFilters ? t('contacts.emptyFiltered') : t('contacts.empty')}
            description={hasFilters ? t('contacts.emptyFilteredHint') : t('contacts.emptyHint')}
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
                      {t('contacts.clearFilters')}
                    </Button>
                  )}
                </div>
              ) : null
            }
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[680px] text-start text-sm">
                <thead className="bg-slate-50 text-xs font-medium text-slate-500">
                  <tr>
                    <th scope="col" className="w-12 px-4 py-3">
                      <input
                        type="checkbox"
                        checked={allOnPageSelected}
                        ref={(element) => {
                          if (element) element.indeterminate = someOnPageSelected
                        }}
                        onChange={toggleAllOnPage}
                        aria-label={t('contacts.selectAllPage')}
                        className="size-4 cursor-pointer rounded border-slate-300 accent-teal-700"
                      />
                    </th>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('contacts.contactName')}
                    </th>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('contacts.students')}
                    </th>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('contacts.studentsCountLabel')}
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {pageItems.map((contact) => (
                    <ContactRow
                      key={contact.id}
                      contact={contact}
                      selected={selected.has(contact.id)}
                      onToggle={() => toggleOne(contact.id)}
                    />
                  ))}
                </tbody>
              </table>
            </div>

            <Pagination
              pageNumber={pageNumber}
              hasNextPage={Boolean(page?.hasNextPage)}
              isFetching={contactsQuery.isFetching}
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

function ContactRow({
  contact,
  selected,
  onToggle,
}: {
  contact: Contact
  selected: boolean
  onToggle: () => void
}) {
  const { t } = useTranslation()
  const displayName = contact.name?.trim() || t('contacts.unnamedContact')

  return (
    <tr
      className={`align-top transition-colors ${selected ? 'bg-teal-50/60' : 'hover:bg-teal-50/40'}`}
    >
      <td className="px-4 py-3">
        <input
          type="checkbox"
          checked={selected}
          onChange={onToggle}
          aria-label={t('contacts.selectRow', { name: displayName })}
          className="mt-0.5 size-4 cursor-pointer rounded border-slate-300 accent-teal-700"
        />
      </td>
      <td className="px-4 py-3">
        <p className="font-medium text-slate-800" dir="auto">
          {displayName}
        </p>
        <p className="mt-1 text-xs text-slate-400">{t('contacts.parentAccount')}</p>
      </td>
      <td className="px-4 py-3">
        {contact.students.length === 0 ? (
          <span className="text-xs text-slate-400">{t('contacts.noStudents')}</span>
        ) : (
          <ul className="flex flex-wrap gap-1.5">
            {contact.students.map((student) => (
              <li
                key={student.id}
                className="rounded-lg border border-slate-200 bg-slate-50 px-2.5 py-1 text-xs text-slate-700"
                dir="auto"
              >
                {student.name?.trim() || '—'}
              </li>
            ))}
          </ul>
        )}
      </td>
      <td className="px-4 py-3">
        <Badge variant={contact.students.length > 0 ? 'info' : 'neutral'}>
          {contact.students.length}
        </Badge>
      </td>
    </tr>
  )
}

function TextField({
  label,
  value,
  onChange,
  dir,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  dir?: 'ltr' | 'rtl'
}) {
  return (
    <label className="space-y-1.5 text-xs font-medium text-slate-500">
      <span>{label}</span>
      <Input dir={dir} value={value} onChange={(event) => onChange(event.target.value)} />
    </label>
  )
}

function SelectField({
  label,
  value,
  options,
  disabled,
  onChange,
}: {
  label: string
  value: string
  options: SelectOption[]
  disabled?: boolean
  onChange: (value: string) => void
}) {
  return (
    <label className="space-y-1.5 text-xs font-medium text-slate-500">
      <span>{label}</span>
      <Select value={value} onValueChange={onChange} options={options} disabled={disabled} />
    </label>
  )
}
