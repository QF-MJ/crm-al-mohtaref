import { useMemo, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { Megaphone, Plus, Search } from 'lucide-react'
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
import { useCampaignLookups, type LookupItem } from '@/features/lookups'
import { useCampaigns } from '../api/campaigns.queries'
import { CampaignStatusBadge } from '../components/CampaignStatusBadge'
import type { CampaignFilters, CampaignListItem } from '../types'

interface DraftFilters {
  title: string
  status: string
  trackId: string
  platformId: string
}

const EMPTY_DRAFT: DraftFilters = { title: '', status: '', trackId: '', platformId: '' }

// يحوّل قائمة lookups إلى خريطة id → name لعرض الأسماء بدل المعرّفات
const toNameMap = (items?: LookupItem[] | null) =>
  new Map((items ?? []).map((item) => [String(item.id), item.name]))

export function CampaignsPage() {
  const { t } = useTranslation()
  const access = useAccess()

  const [draft, setDraft] = useState<DraftFilters>(EMPTY_DRAFT)
  const [filters, setFilters] = useState<CampaignFilters>({ pageNumber: 1 })

  const lookupsQuery = useCampaignLookups()
  const campaignsQuery = useCampaigns(filters)

  const lookups = lookupsQuery.data
  const trackNames = useMemo(() => toNameMap(lookups?.tracks), [lookups?.tracks])
  const statusNames = useMemo(
    () => toNameMap(lookups?.campaignStatuses),
    [lookups?.campaignStatuses],
  )

  const lookupsBusy = lookupsQuery.isLoading || lookupsQuery.isError
  // الباك الحالي لا يعيد حالات الحملة والمنصات: نعطّل فلاترها بدل عرض قائمة فارغة
  const statusesAvailable = Boolean(lookups?.campaignStatuses?.length)
  const platformsAvailable = Boolean(lookups?.platforms?.length)

  const buildOptions = (allLabel: string, items?: LookupItem[] | null): SelectOption[] => [
    { value: '', label: allLabel },
    ...(items ?? []).map((item) => ({ value: String(item.id), label: item.name })),
  ]

  const applyFilters = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFilters({
      pageNumber: 1,
      title: draft.title.trim() || undefined,
      status: draft.status || undefined,
      trackId: draft.trackId || undefined,
      platformId: draft.platformId || undefined,
    })
  }

  const clearFilters = () => {
    setDraft(EMPTY_DRAFT)
    setFilters({ pageNumber: 1 })
  }

  const goToPage = (pageNumber: number) =>
    setFilters((current) => ({ ...current, pageNumber: Math.max(1, pageNumber) }))

  const hasFilters = Boolean(
    filters.title || filters.status || filters.trackId || filters.platformId,
  )
  const pageNumber = filters.pageNumber
  const items = campaignsQuery.data?.items ?? []

  return (
    <section className="space-y-5">
      <PageHeader
        breadcrumb={t('campaigns.breadcrumb')}
        title={t('campaigns.title')}
        description={t('campaigns.subtitle')}
        actions={
          access.can('campaigns.create') ? (
            <Link
              to={paths.campaignsNew}
              className="inline-flex h-9 items-center gap-2 rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground hover:bg-primary/90"
            >
              <Plus aria-hidden="true" className="size-4" />
              {t('campaigns.create')}
            </Link>
          ) : null
        }
      />

      <form
        noValidate
        onSubmit={applyFilters}
        aria-label={t('campaigns.filters')}
        className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"
      >
        <h2 className="mb-3 font-semibold text-slate-900">{t('campaigns.filters')}</h2>
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
          <label className="space-y-1.5 text-xs font-medium text-slate-500">
            <span>{t('campaigns.campaignTitle')}</span>
            <Input
              dir="auto"
              value={draft.title}
              placeholder={t('campaigns.titlePlaceholder')}
              onChange={(event) =>
                setDraft((current) => ({ ...current, title: event.target.value }))
              }
            />
          </label>
          <SelectField
            label={t('campaigns.status')}
            value={draft.status}
            options={buildOptions(t('campaigns.allStatuses'), lookups?.campaignStatuses)}
            disabled={lookupsBusy || !statusesAvailable}
            onChange={(status) => setDraft((current) => ({ ...current, status }))}
          />
          <SelectField
            label={t('campaigns.track')}
            value={draft.trackId}
            options={buildOptions(t('campaigns.allTracks'), lookups?.tracks)}
            disabled={lookupsBusy}
            onChange={(trackId) => setDraft((current) => ({ ...current, trackId }))}
          />
          <SelectField
            label={t('campaigns.platform')}
            value={draft.platformId}
            options={buildOptions(t('campaigns.allPlatforms'), lookups?.platforms)}
            disabled={lookupsBusy || !platformsAvailable}
            onChange={(platformId) => setDraft((current) => ({ ...current, platformId }))}
          />
        </div>

        {lookupsQuery.isError && (
          <p role="alert" className="mt-3 text-sm text-destructive">
            {t('campaigns.lookupsError')}
          </p>
        )}
        {lookupsQuery.isSuccess && (!statusesAvailable || !platformsAvailable) && (
          <p className="mt-3 text-xs text-slate-400">{t('campaigns.lookupsPartial')}</p>
        )}

        <div className="mt-4 flex flex-wrap justify-center gap-2">
          <Button type="submit" size="sm">
            <Search aria-hidden="true" />
            {t('actions.search')}
          </Button>
          <Button type="button" variant="outline" size="sm" onClick={clearFilters}>
            {t('campaigns.clearFilters')}
          </Button>
        </div>
      </form>

      <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
        <header className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 px-4 py-4 sm:px-5">
          <div>
            <h2 className="font-semibold text-slate-900">{t('campaigns.results')}</h2>
            <p className="mt-1 text-xs leading-6 text-slate-500">
              {t('campaigns.resultsDescription')}
            </p>
          </div>
          {campaignsQuery.data && (
            <Badge variant="neutral">{t('campaigns.pageCount', { count: items.length })}</Badge>
          )}
        </header>

        {campaignsQuery.isLoading ? (
          <TableSkeleton rows={6} columns={4} />
        ) : campaignsQuery.isError ? (
          <ErrorState
            error={campaignsQuery.error}
            onRetry={() => void campaignsQuery.refetch()}
            isRetrying={campaignsQuery.isFetching}
          />
        ) : items.length === 0 ? (
          <EmptyState
            icon={Megaphone}
            title={hasFilters ? t('campaigns.emptyFiltered') : t('campaigns.empty')}
            description={hasFilters ? t('campaigns.emptyFilteredHint') : t('campaigns.emptyHint')}
            action={
              pageNumber > 1 || hasFilters ? (
                <div className="flex flex-wrap justify-center gap-2">
                  {pageNumber > 1 && (
                    <Button type="button" size="sm" onClick={() => goToPage(pageNumber - 1)}>
                      {t('pagination.previous')}
                    </Button>
                  )}
                  {hasFilters && (
                    <Button type="button" variant="outline" size="sm" onClick={clearFilters}>
                      {t('campaigns.clearFilters')}
                    </Button>
                  )}
                </div>
              ) : null
            }
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[640px] text-start text-sm">
                <thead className="bg-slate-50 text-xs font-medium text-slate-500">
                  <tr>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('campaigns.campaignTitle')}
                    </th>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('campaigns.track')}
                    </th>
                    <th scope="col" className="px-4 py-3 text-start">
                      {t('campaigns.status')}
                    </th>
                    <th scope="col" className="px-4 py-3 text-start">
                      <span className="sr-only">{t('campaigns.actions')}</span>
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {items.map((campaign) => (
                    <CampaignRow
                      key={campaign.id}
                      campaign={campaign}
                      trackName={trackNames.get(campaign.trackId)}
                      statusName={statusNames.get(campaign.statusId)}
                    />
                  ))}
                </tbody>
              </table>
            </div>

            <Pagination
              pageNumber={pageNumber}
              hasNextPage={Boolean(campaignsQuery.data?.page?.hasNextPage)}
              isFetching={campaignsQuery.isFetching}
              onPrevious={() => goToPage(pageNumber - 1)}
              onNext={() => goToPage(pageNumber + 1)}
            />
          </>
        )}
      </section>
    </section>
  )
}

function CampaignRow({
  campaign,
  trackName,
  statusName,
}: {
  campaign: CampaignListItem
  trackName?: string
  statusName?: string
}) {
  const { t } = useTranslation()
  const title = campaign.title?.trim() || t('campaigns.untitled')

  return (
    <tr className="transition-colors hover:bg-teal-50/50">
      <td className="px-4 py-3 font-medium text-slate-800">
        <Link
          to={paths.campaign(campaign.id)}
          className="hover:text-primary hover:underline"
          dir="auto"
          title={title}
        >
          {title}
        </Link>
      </td>
      <td className="px-4 py-3 text-slate-600" dir="auto">
        {trackName ?? '—'}
      </td>
      <td className="px-4 py-3">
        <CampaignStatusBadge name={statusName} />
      </td>
      <td className="px-4 py-3 text-end">
        <Link
          to={paths.campaign(campaign.id)}
          className="inline-flex h-8 items-center rounded-lg border border-slate-200 bg-white px-3 text-xs font-medium text-slate-700 hover:bg-slate-50"
        >
          {t('actions.view')}
        </Link>
      </td>
    </tr>
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
