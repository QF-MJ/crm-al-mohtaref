import type { KeyboardEvent } from 'react'
import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { AlertTriangle, ArrowRight, ChevronLeft, Info, LockKeyhole } from 'lucide-react'
import { toApiError } from '@/api/errors'
import { Badge, type BadgeVariant } from '@/components/ui/badge'
import { ErrorState } from '@/components/common/ErrorState'
import { PageHeader } from '@/components/common/PageHeader'
import { paths } from '@/app/router/paths'
import type {
  ContactProfile,
  ContactProfileSection,
  ProfileSectionKey,
  ProfileSectionState,
} from '../types'
import { useContactProfile } from '../api/contacts.profile.queries'

const SECTION_ORDER = [
  'opportunities',
  'communications',
  'subscriptions',
  'guidance',
  'support',
] as const

const STATE_VARIANT: Record<ProfileSectionState, BadgeVariant> = {
  loaded: 'success',
  empty: 'warning',
  unavailable: 'danger',
}

export function CustomerProfilePage() {
  const { t } = useTranslation()
  const { clientId } = useParams<{ clientId: string }>()
  const profileQuery = useContactProfile(clientId)
  const [selectedStudentId, setSelectedStudentId] = useState<string>()

  const header = (
    <PageHeader
      breadcrumb={t('contacts.profile.breadcrumb')}
      title={t('contacts.profile.title')}
      description={t('contacts.profile.subtitle')}
      actions={<Badge variant="info">{t('contacts.profile.readOnly')}</Badge>}
    />
  )

  if (!clientId) {
    return (
      <section className="space-y-5">
        {header}
        <ErrorState error={{ status: 404, message: t('contacts.profile.notFound') }} />
        <BackLink />
      </section>
    )
  }

  if (profileQuery.isLoading) {
    return (
      <section className="space-y-5">
        {header}
        <div className="space-y-3 rounded-lg border border-slate-200 bg-white p-6" aria-busy="true">
          {Array.from({ length: 6 }, (_unused, index) => (
            <div key={index} className="h-9 animate-pulse rounded-md bg-slate-100" />
          ))}
        </div>
      </section>
    )
  }

  if (profileQuery.isError || !profileQuery.data) {
    const status = toApiError(profileQuery.error).status
    return (
      <section className="space-y-5">
        {header}
        {status === 404 || status === 403 ? (
          <div className="rounded-lg border border-slate-200 bg-white p-6">
            <div className="flex flex-col items-center gap-3 text-center">
              <span className="grid size-12 place-items-center rounded-lg bg-amber-50 text-amber-700 ring-1 ring-inset ring-amber-200">
                <AlertTriangle aria-hidden="true" className="size-6" />
              </span>
              <p className="text-sm font-semibold text-slate-800">
                {status === 404
                  ? t('contacts.profile.notFound')
                  : t('contacts.profile.accessDenied')}
              </p>
              <p className="max-w-md text-sm leading-6 text-slate-500">
                {status === 404
                  ? t('contacts.profile.notFoundHint')
                  : t('contacts.profile.accessDeniedHint')}
              </p>
            </div>
          </div>
        ) : (
          <div className="rounded-lg border border-slate-200 bg-white">
            <ErrorState
              error={profileQuery.error}
              onRetry={() => void profileQuery.refetch()}
              isRetrying={profileQuery.isFetching}
            />
          </div>
        )}
        <BackLink />
      </section>
    )
  }

  const profile = profileQuery.data
  const students = profile.students
  const activeStudent = students.find((student) => student.id === selectedStudentId) ?? students[0]
  const sectionsByKey = new Map(
    activeStudent?.sections.map((section) => [section.key, section]) ?? [],
  )

  function handleTabKeyDown(event: KeyboardEvent<HTMLButtonElement>, index: number) {
    let nextIndex: number | undefined
    // RTL: السهم لليسار يتقدّم بصرياً بين التبويبات
    if (event.key === 'ArrowLeft') nextIndex = (index + 1) % students.length
    if (event.key === 'ArrowRight') nextIndex = (index - 1 + students.length) % students.length
    if (event.key === 'Home') nextIndex = 0
    if (event.key === 'End') nextIndex = students.length - 1
    if (nextIndex === undefined) return
    event.preventDefault()
    const nextStudent = students[nextIndex]
    setSelectedStudentId(nextStudent.id)
    document.getElementById(`contact-student-tab-${nextStudent.id}`)?.focus()
  }

  return (
    <section className="space-y-5">
      {header}

      <IdentityCard profile={profile} />

      <div className="grid gap-4 lg:grid-cols-[212px_minmax(0,1fr)]">
        <SectionNav sectionsByKey={sectionsByKey} />

        <div className="min-w-0 rounded-lg border border-slate-200 bg-white p-4 sm:p-5">
          <div className="flex flex-wrap items-end justify-between gap-2">
            <h2 className="text-base font-semibold text-slate-900">
              {t('contacts.profile.students')}
            </h2>
            <p className="text-xs leading-6 text-slate-500">
              {t('contacts.profile.studentsHint')}
            </p>
          </div>

          {students.length === 0 ? (
            <div className="mt-4 flex items-start gap-2 rounded-md border-s-2 border-primary bg-slate-50 px-3 py-3 text-xs leading-6 text-slate-600">
              <Info aria-hidden="true" className="mt-0.5 size-4 shrink-0 text-primary" />
              <span>
                <b className="font-semibold text-slate-800">
                  {t('contacts.profile.noStudents')}
                </b>{' '}
                {t('contacts.profile.noStudentsHint')}
              </span>
            </div>
          ) : (
            <>
              <div className="mt-3 space-y-1.5 border-s border-slate-200 ps-5">
                <p className="flex items-center gap-2 text-xs font-semibold text-slate-600">
                  <span aria-hidden="true" className="size-2.5 rounded-full border-2 border-primary bg-primary" />
                  <span dir="auto">{profile.name}</span>
                </p>
                <div className="space-y-1.5" role="tablist" aria-label={t('contacts.profile.students')}>
                  {students.map((student, index) => {
                    const selected = activeStudent?.id === student.id
                    return (
                      <button
                        key={student.id}
                        id={`contact-student-tab-${student.id}`}
                        type="button"
                        role="tab"
                        aria-selected={selected}
                        aria-controls="contact-profile-panels"
                        tabIndex={selected ? 0 : -1}
                        onClick={() => setSelectedStudentId(student.id)}
                        onKeyDown={(event) => handleTabKeyDown(event, index)}
                        className={`relative flex w-full items-center gap-2 rounded-lg border px-3 py-2 text-start transition-colors ${
                          selected
                            ? 'border-primary bg-teal-50/60'
                            : 'border-slate-200 bg-slate-50/60 hover:bg-slate-100'
                        }`}
                      >
                        <span
                          aria-hidden="true"
                          className="absolute -start-5 top-1/2 h-px w-4 bg-slate-200"
                        />
                        <span
                          aria-hidden="true"
                          className={`size-2.5 shrink-0 rounded-full border-2 border-primary ${
                            selected ? 'bg-primary' : 'bg-white'
                          }`}
                        />
                        <span className="min-w-0 flex-1">
                          <span className="block truncate text-sm font-semibold text-slate-800" dir="auto">
                            {student.name}
                          </span>
                          {student.grade && (
                            <span className="block text-xs text-slate-500" dir="auto">
                              {student.grade}
                            </span>
                          )}
                        </span>
                        <Badge variant={selected ? 'info' : 'neutral'}>
                          {t('contacts.profile.recordCount', { count: student.sections.length })}
                        </Badge>
                      </button>
                    )
                  })}
                </div>
              </div>

              <div className="mt-3 flex items-start gap-2 rounded-md border-s-2 border-primary bg-slate-50 px-3 py-2 text-xs leading-6 text-slate-600">
                <Info aria-hidden="true" className="mt-0.5 size-4 shrink-0 text-primary" />
                <span>
                  {t('contacts.profile.scopeNote', { name: activeStudent?.name ?? '' })}
                </span>
              </div>

              <div
                id="contact-profile-panels"
                role="tabpanel"
                aria-labelledby={
                  activeStudent ? `contact-student-tab-${activeStudent.id}` : undefined
                }
                className="mt-3"
              >
                {activeStudent &&
                  SECTION_ORDER.map((key) => (
                    <ProfileSectionCard
                      key={key}
                      sectionKey={key}
                      section={sectionsByKey.get(key)}
                    />
                  ))}
              </div>
            </>
          )}
        </div>
      </div>

      <div className="flex flex-wrap items-center gap-3 text-xs text-slate-500">
        <LockKeyhole aria-hidden="true" className="size-4" />
        <span>{t('contacts.profile.readOnlyHint')}</span>
      </div>

      <BackLink />
    </section>
  )
}

function IdentityCard({ profile }: { profile: ContactProfile }) {
  const { t } = useTranslation()
  const initial = profile.name.trim().charAt(0) || '—'

  return (
    <section className="rounded-lg border border-slate-200 bg-white p-5">
      <div className="flex flex-wrap items-start gap-3">
        <span
          aria-hidden="true"
          className="grid size-12 shrink-0 place-items-center rounded-xl bg-teal-50 text-lg font-bold text-teal-800"
        >
          {initial}
        </span>
        <div className="min-w-0 flex-1">
          <h2 className="text-xl font-bold leading-tight text-slate-900" dir="auto">
            {profile.name}
          </h2>
          <p className="mt-1 text-xs leading-6 text-slate-500">
            {t('contacts.profile.contactHint')}
          </p>
          <div className="mt-2 flex flex-wrap gap-2">
            <Badge variant="info">{t('contacts.profile.studentsCount', { count: profile.students.length })}</Badge>
          </div>
        </div>
      </div>
      <dl className="mt-4 grid gap-3 border-t border-slate-200 pt-4 sm:grid-cols-2 lg:grid-cols-4">
        <DetailItem label={t('contacts.profile.phone')} value={profile.phoneNumber} ltr />
        <DetailItem label={t('contacts.profile.email')} value={profile.email} ltr />
        <DetailItem label={t('contacts.profile.city')} value={profile.city} />
        <DetailItem label={t('contacts.profile.lastUpdated')} value={profile.updatedAt} ltr />
      </dl>
    </section>
  )
}

function SectionNav({
  sectionsByKey,
}: {
  sectionsByKey: Map<ProfileSectionKey, ContactProfileSection>
}) {
  const { t } = useTranslation()

  return (
    <aside className="rounded-lg border border-slate-200 bg-white p-3 lg:sticky lg:top-3">
      <h2 className="mb-2 px-2 text-xs font-semibold text-slate-500">
        {t('contacts.profile.sectionsNav')}
      </h2>
      <ol className="space-y-0.5">
        <li>
          <a
            href="#contact-profile-panels"
            className="flex items-center gap-2 rounded-md bg-slate-50 px-2 py-1.5 text-xs font-semibold text-slate-800"
          >
            <span aria-hidden="true" className="size-1.5 rounded-full bg-primary" />
            {t('contacts.profile.students')}
          </a>
        </li>
        {SECTION_ORDER.map((key) => {
          const state = sectionsByKey.get(key)?.state
          const dotClass =
            state === 'loaded'
              ? 'bg-primary'
              : state === 'unavailable'
                ? 'bg-rose-600'
                : 'bg-amber-400'
          return (
            <li key={key}>
              <a
                href={`#contact-section-${key}`}
                className="flex items-center gap-2 rounded-md px-2 py-1.5 text-xs text-slate-600 hover:bg-slate-50 hover:text-slate-900"
              >
                <span aria-hidden="true" className={`size-1.5 shrink-0 rounded-full ${dotClass}`} />
                {t(`contacts.profile.sections.${key}`)}
              </a>
            </li>
          )
        })}
      </ol>
      <p className="mt-3 border-s-2 border-slate-200 ps-2 text-xs leading-5 text-slate-500">
        {t('contacts.profile.sectionNote')}
      </p>
    </aside>
  )
}

function ProfileSectionCard({
  sectionKey,
  section,
}: {
  sectionKey: (typeof SECTION_ORDER)[number]
  section?: ContactProfileSection
}) {
  const { t } = useTranslation()
  const state = section?.state ?? 'unavailable'
  const rows = section?.rows ?? []

  return (
    <details
      id={`contact-section-${sectionKey}`}
      className="mb-2.5 overflow-hidden rounded-md border border-slate-200"
      open
    >
      <summary className="flex cursor-pointer items-center gap-2 bg-slate-50 px-3 py-2.5 [&::-webkit-details-marker]:hidden">
        <ChevronLeft aria-hidden="true" className="size-4 text-slate-400" />
        <h3 className="m-0 text-sm font-semibold text-slate-900">
          {t(`contacts.profile.sections.${sectionKey}`)}
        </h3>
        <Badge variant={STATE_VARIANT[state]} className="ms-auto">
          {t(`contacts.profile.states.${state}`)}
        </Badge>
      </summary>
      <div className="px-3 py-2">
        {state === 'unavailable' ? (
          <p className="flex items-start gap-2 py-3 text-xs leading-6 text-slate-600">
            <AlertTriangle aria-hidden="true" className="mt-0.5 size-4 shrink-0 text-rose-600" />
            {t('contacts.profile.unavailableSection')}
          </p>
        ) : rows.length === 0 ? (
          <p className="flex items-start gap-2 py-3 text-xs leading-6 text-slate-600">
            <Info aria-hidden="true" className="mt-0.5 size-4 shrink-0 text-primary" />
            {t('contacts.profile.emptySection')}
          </p>
        ) : (
          <div className="divide-y divide-slate-200">
            {rows.map((row, index) => (
              <div
                key={`${row.title}-${index}`}
                className="grid gap-1 py-2 text-xs sm:grid-cols-[auto_minmax(0,1fr)_auto] sm:items-start sm:gap-3"
              >
                {row.date && (
                  <span dir="ltr" className="whitespace-nowrap text-slate-400">
                    {row.date}
                  </span>
                )}
                <div className="min-w-0">
                  <p className="font-semibold text-slate-800" dir="auto">
                    {row.title}
                  </p>
                  {row.description && (
                    <p className="mt-0.5 leading-6 text-slate-600" dir="auto">
                      {row.description}
                    </p>
                  )}
                </div>
                {row.meta && (
                  <Badge variant="neutral" className="w-fit">
                    {row.meta}
                  </Badge>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </details>
  )
}

function DetailItem({
  label,
  value,
  ltr = false,
}: {
  label: string
  value?: string
  ltr?: boolean
}) {
  return (
    <div className="space-y-1">
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd className="truncate text-sm font-medium text-slate-800" dir={ltr ? 'ltr' : 'auto'} title={value}>
        {value?.trim() || '—'}
      </dd>
    </div>
  )
}

function BackLink() {
  const { t } = useTranslation()
  return (
    <Link
      to={paths.clients}
      className="inline-flex w-fit items-center gap-2 text-sm font-medium text-primary hover:underline"
    >
      <ArrowRight aria-hidden="true" className="size-4" />
      {t('contacts.profile.backToList')}
    </Link>
  )
}
