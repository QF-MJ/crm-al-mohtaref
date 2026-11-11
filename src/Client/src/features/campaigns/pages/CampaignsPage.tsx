import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Plus, Search, ChevronDown } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select, type SelectOption } from '@/components/ui/select'
import { getApiErrorMessage } from '@/api/errors'
import { paths } from '@/app/router/paths'
import { usePermission } from '@/permissions/usePermission'
import { useLookups } from '@/features/lookups/api/lookups.queries'
import { CampaignStatusBadge } from '../components/CampaignStatusBadge'
import { useCampaigns } from '../api/campaigns.queries'
import type { CampaignFilters, CampaignListItem } from '../types'

function errorText(error: unknown, fallback: string) {
  return getApiErrorMessage(error, fallback)
}

export function CampaignsPage() {
  const { t } = useTranslation()
  const can = usePermission()
  const [filters, setFilters] = useState<CampaignFilters>({ pageNumber: 1 })
  const [form, setForm] = useState({ title: '', status: '', trackId: '', platformId: '' })
  const campaignsQuery = useCampaigns(filters)
  const lookupsQuery = useLookups()
  const page = campaignsQuery.data?.page

  const applyFilters = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFilters({
      pageNumber: 1,
      title: form.title.trim() || undefined,
      status: form.status ? Number(form.status) : undefined,
      trackId: form.trackId ? Number(form.trackId) : undefined,
      platformId: form.platformId ? Number(form.platformId) : undefined,
    })
  }

  const clearFilters = () => {
    setForm({ title: '', status: '', trackId: '', platformId: '' })
    setFilters({ pageNumber: 1 })
  }

  return (
    <section className="-m-4 min-h-full space-y-5 bg-slate-50 p-4 sm:-m-6 sm:p-6" dir="rtl">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="mb-1 text-xs text-slate-400">{t('campaigns.breadcrumb')}</p>
          <h1 className="text-2xl font-bold text-slate-800">{t('campaigns.title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('campaigns.subtitle')}</p>
        </div>
        {can('campaigns.create') && (
          <div className="space-y-1 text-end">
            <Button type="button" disabled title={t('campaigns.createUnavailable')}>
              <Plus aria-hidden="true" />
              {t('campaigns.create')}
            </Button>
            <p className="text-xs text-slate-500">{t('campaigns.createUnavailable')}</p>
          </div>
        )}
      </header>

      <form onSubmit={applyFilters} className="space-y-4 rounded-xl border bg-white p-4 sm:p-5">
        <div className="flex items-center justify-between">
          <h2 className="font-semibold text-slate-800">{t('campaigns.filtersTitle')}</h2>
          <Search className="size-4 text-slate-400" aria-hidden="true" />
        </div>
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
          <label className="space-y-1.5 text-xs font-medium text-slate-500">
            <span>{t('campaigns.titleField')}</span>
            <Input
              value={form.title}
              onChange={(event) => setForm((current) => ({ ...current, title: event.target.value }))}
              placeholder={t('campaigns.searchTitle')}
            />
          </label>
          <SelectField
            label={t('campaigns.status')}
            value={form.status}
            onChange={(status) => setForm((current) => ({ ...current, status }))}
            options={[
              { value: '', label: t('campaigns.allStatuses') },
              ...(lookupsQuery.data?.campaignStatuses ?? []).map((status) => ({
                value: String(status.id),
                label: status.name,
              })),
            ]}
          />
          <SelectField
            label={t('campaigns.track')}
            value={form.trackId}
            onChange={(trackId) => setForm((current) => ({ ...current, trackId }))}
            options={[
              { value: '', label: t('campaigns.allTracks') },
              ...(lookupsQuery.data?.tracks ?? []).map((track) => ({
                value: String(track.id),
                label: track.name,
              })),
            ]}
          />
          <SelectField
            label={t('campaigns.platform')}
            value={form.platformId}
            onChange={(platformId) => setForm((current) => ({ ...current, platformId }))}
            options={[
              { value: '', label: t('campaigns.allPlatforms') },
              ...(lookupsQuery.data?.platforms ?? []).map((platform) => ({
                value: String(platform.id),
                label: platform.name,
              })),
            ]}
          />
        </div>
        {lookupsQuery.isError && (
          <p className="text-sm text-destructive" role="alert">
            {errorText(lookupsQuery.error, t('campaigns.error'))}
          </p>
        )}
        <div className="flex justify-center gap-2">
          <Button type="submit" size="sm">
            <Search aria-hidden="true" />
            {t('campaigns.search')}
          </Button>
          <Button type="button" variant="outline" size="sm" onClick={clearFilters}>
            {t('campaigns.clearFilters')}
          </Button>
        </div>
      </form>

      <section className="overflow-hidden rounded-xl border bg-white">
        <header className="flex flex-wrap items-center justify-between gap-3 border-b px-4 py-4 sm:px-5">
          <div>
            <h2 className="font-semibold text-slate-800">{t('campaigns.listTitle')}</h2>
            <p className="mt-1 text-xs text-slate-500">{t('campaigns.listDescription')}</p>
          </div>
          {campaignsQuery.data && (
            <span className="rounded-md bg-slate-100 px-3 py-2 text-xs text-slate-600">
              {t('campaigns.count', { count: campaignsQuery.data.items.length })}
            </span>
          )}
        </header>
        {campaignsQuery.isLoading ? (
          <p className="p-8 text-center text-sm text-slate-500" role="status">
            {t('campaigns.loading')}
          </p>
        ) : campaignsQuery.isError ? (
          <div className="space-y-3 p-8 text-center" role="alert">
            <p className="text-sm text-destructive">
              {errorText(campaignsQuery.error, t('campaigns.error'))}
            </p>
            <Button
              type="button"
              variant="outline"
              onClick={() => void campaignsQuery.refetch()}
              disabled={campaignsQuery.isFetching}
            >
              {t('campaigns.retry')}
            </Button>
          </div>
        ) : !campaignsQuery.data ? (
          <div className="space-y-3 p-8 text-center" role="alert">
            <p className="text-sm text-destructive">{t('campaigns.error')}</p>
            <Button
              type="button"
              variant="outline"
              onClick={() => void campaignsQuery.refetch()}
              disabled={campaignsQuery.isFetching}
            >
              {t('campaigns.retry')}
            </Button>
          </div>
        ) : !campaignsQuery.data.items.length ? (
          <p className="p-8 text-center text-sm text-slate-500">{t('campaigns.empty')}</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[680px] text-start text-sm">
              <thead className="bg-slate-50 text-xs font-medium text-slate-500">
                <tr>
                  <th className="px-4 py-3">{t('campaigns.titleField')}</th>
                  <th className="px-4 py-3">{t('campaigns.track')}</th>
                  <th className="px-4 py-3">{t('campaigns.status')}</th>
                  <th className="px-4 py-3">{t('campaigns.id')}</th>
                  <th className="px-4 py-3">{t('campaigns.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {campaignsQuery.data.items.map((campaign) => (
                  <CampaignRow
                    key={campaign.id}
                    campaign={campaign}
                    trackName={lookupsQuery.data?.tracks?.find((item) => item.id === campaign.trackId)?.name}
                    statusName={
                      lookupsQuery.data?.campaignStatuses?.find(
                        (item) => item.id === campaign.status,
                      )?.name
                    }
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}
        {campaignsQuery.data && (
          <footer className="flex items-center justify-between border-t px-4 py-3 text-xs text-slate-500">
            <span>{t('campaigns.page', { page: page?.pageNumber ?? filters.pageNumber })}</span>
            <div className="flex gap-2">
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={(page?.pageNumber ?? 1) <= 1 || campaignsQuery.isFetching}
                onClick={() =>
                  setFilters((current) => ({
                    ...current,
                    pageNumber: Math.max(1, current.pageNumber - 1),
                  }))
                }
              >
                {t('campaigns.previous')}
              </Button>
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={!page?.hasNextPage || campaignsQuery.isFetching}
                onClick={() =>
                  setFilters((current) => ({ ...current, pageNumber: current.pageNumber + 1 }))
                }
              >
                {t('campaigns.next')}
              </Button>
            </div>
          </footer>
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
  return (
    <tr className="transition-colors hover:bg-teal-50/50">
      <td className="px-4 py-3 font-semibold text-slate-700">
        <Link
          to={paths.campaignEdit(campaign.id)}
          className="inline-flex items-center gap-2 hover:text-primary"
        >
          {campaign.title}
          <ChevronDown className="size-3 -rotate-90" aria-hidden="true" />
        </Link>
      </td>
      <td className="px-4 py-3 text-slate-600">{trackName ?? '—'}</td>
      <td className="px-4 py-3">
        <CampaignStatusBadge label={statusName} status={campaign.status} />
      </td>
      <td className="px-4 py-3 text-slate-500">{campaign.id}</td>
      <td className="px-4 py-3">
        <Link
          to={paths.campaignEdit(campaign.id)}
          className="text-sm font-medium text-primary hover:underline"
        >
          {t('campaigns.review')}
        </Link>
      </td>
    </tr>
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
  options: SelectOption[]
}) {
  return (
    <label className="space-y-1.5 text-xs font-medium text-slate-500">
      <span>{label}</span>
      <Select value={value} onValueChange={onChange} options={options} />
    </label>
  )
}
