import { useMemo, useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ArrowLeft, ChevronDown, Plus, Search } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { paths } from '@/app/router/paths'
import { usePermission } from '@/permissions/usePermission'
import { CampaignStatusBadge } from '../components/CampaignStatusBadge'
import { campaignTracks, getMockCampaigns } from '../mock-data'
import type { Campaign, CampaignStatus } from '../types'

export function CampaignsPage() {
  const { t } = useTranslation()
  const can = usePermission()
  const campaigns = getMockCampaigns()
  const [title, setTitle] = useState('')
  const [status, setStatus] = useState('')
  const [trackId, setTrackId] = useState('')
  const [platform, setPlatform] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 5

  const filteredCampaigns = useMemo(
    () =>
      campaigns.filter(
        (campaign) =>
          (!title.trim() ||
            campaign.title.toLocaleLowerCase().includes(title.trim().toLocaleLowerCase())) &&
          (!status || campaign.status === status) &&
          (!trackId || campaign.trackId === Number(trackId)) &&
          (!platform || campaign.platforms.includes(platform)),
      ),
    [campaigns, platform, status, title, trackId],
  )
  const visibleCampaigns = filteredCampaigns.slice((page - 1) * pageSize, page * pageSize)
  const hasNextPage = page * pageSize < filteredCampaigns.length

  const resetFilters = () => {
    setTitle('')
    setStatus('')
    setTrackId('')
    setPlatform('')
    setPage(1)
  }

  return (
    <section className="-m-6 min-h-full space-y-5 bg-slate-50 p-5 sm:p-6" dir="rtl">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="mb-1 text-xs text-slate-400">{t('campaigns.breadcrumb')}</p>
          <h1 className="text-2xl font-bold text-slate-800">{t('campaigns.title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('campaigns.subtitle')}</p>
        </div>
        {can('campaigns.create') && (
          <Link
            to={paths.campaignsNew}
            className="inline-flex h-9 items-center justify-center gap-2 rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground hover:opacity-90"
          >
            <Plus aria-hidden="true" />
            {t('campaigns.create')}
          </Link>
        )}
      </header>

      <section className="space-y-4 rounded-xl border bg-white p-4 sm:p-5">
        <div className="flex items-center justify-between">
          <h2 className="font-semibold text-slate-800">{t('campaigns.filtersTitle')}</h2>
          <Search className="size-4 text-slate-400" aria-hidden="true" />
        </div>
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
          <FilterField
            label={t('campaigns.titleField')}
            value={title}
            onChange={(value) => {
              setTitle(value)
              setPage(1)
            }}
            placeholder={t('campaigns.searchTitle')}
          />
          <SelectField
            label={t('campaigns.status')}
            value={status}
            onChange={(value) => {
              setStatus(value)
              setPage(1)
            }}
          >
            <option value="">{t('campaigns.allStatuses')}</option>
            {(['draft', 'active', 'paused', 'completed'] as CampaignStatus[]).map((value) => (
              <option key={value} value={value}>
                {t(`campaigns.statuses.${value}`)}
              </option>
            ))}
          </SelectField>
          <SelectField
            label={t('campaigns.track')}
            value={trackId}
            onChange={(value) => {
              setTrackId(value)
              setPage(1)
            }}
          >
            <option value="">{t('campaigns.allTracks')}</option>
            {campaignTracks.map((track) => (
              <option key={track.id} value={track.id}>
                {track.name}
              </option>
            ))}
          </SelectField>
          <SelectField
            label={t('campaigns.platform')}
            value={platform}
            onChange={(value) => {
              setPlatform(value)
              setPage(1)
            }}
          >
            <option value="">{t('campaigns.allPlatforms')}</option>
            {['Facebook', 'Instagram', 'TikTok', 'Google'].map((value) => (
              <option key={value} value={value}>
                {value}
              </option>
            ))}
          </SelectField>
        </div>
        <div className="flex justify-center">
          <Button type="button" variant="outline" size="sm" onClick={resetFilters}>
            {t('campaigns.clearFilters')}
          </Button>
        </div>
      </section>

      <section className="overflow-hidden rounded-xl border bg-white">
        <header className="flex flex-wrap items-center justify-between gap-3 border-b px-4 py-4 sm:px-5">
          <div>
            <h2 className="font-semibold text-slate-800">{t('campaigns.listTitle')}</h2>
            <p className="mt-1 text-xs text-slate-500">{t('campaigns.listDescription')}</p>
          </div>
          <span className="rounded-md bg-slate-100 px-3 py-2 text-xs text-slate-600">
            {t('campaigns.count', { count: filteredCampaigns.length })}
          </span>
        </header>
        {visibleCampaigns.length === 0 ? (
          <p className="p-8 text-center text-sm text-slate-500">{t('campaigns.empty')}</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[720px] text-start text-sm">
              <thead className="bg-slate-50 text-xs font-medium text-slate-500">
                <tr>
                  <th scope="col" className="px-4 py-3">{t('campaigns.titleField')}</th>
                  <th scope="col" className="px-4 py-3">{t('campaigns.track')}</th>
                  <th scope="col" className="px-4 py-3">{t('campaigns.platform')}</th>
                  <th scope="col" className="px-4 py-3">{t('campaigns.status')}</th>
                  <th scope="col" className="px-4 py-3">{t('campaigns.id')}</th>
                  <th scope="col" className="px-4 py-3">{t('campaigns.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {visibleCampaigns.map((campaign) => (
                  <CampaignRow key={campaign.id} campaign={campaign} />
                ))}
              </tbody>
            </table>
          </div>
        )}
        <footer className="flex items-center justify-between border-t px-4 py-3 text-xs text-slate-500">
          <span>{t('campaigns.page', { page })}</span>
          <div className="flex gap-2">
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={page === 1}
              onClick={() => setPage((current) => Math.max(1, current - 1))}
            >
              {t('campaigns.previous')}
            </Button>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={!hasNextPage}
              onClick={() => setPage((current) => current + 1)}
            >
              {t('campaigns.next')}
              <ArrowLeft aria-hidden="true" />
            </Button>
          </div>
        </footer>
      </section>

    </section>
  )
}

function CampaignRow({ campaign }: { campaign: Campaign }) {
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
      <td className="px-4 py-3 text-slate-600">{campaign.trackName}</td>
      <td className="px-4 py-3 text-slate-600">{campaign.platforms.join('، ') || '—'}</td>
      <td className="px-4 py-3">
        <CampaignStatusBadge status={campaign.status} />
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

function FilterField({
  label,
  value,
  onChange,
  placeholder,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  placeholder: string
}) {
  return (
    <label className="space-y-1.5 text-xs font-medium text-slate-500">
      <span>{label}</span>
      <Input value={value} onChange={(event) => onChange(event.target.value)} placeholder={placeholder} />
    </label>
  )
}

function SelectField({
  label,
  value,
  onChange,
  children,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  children: ReactNode
}) {
  return (
    <label className="space-y-1.5 text-xs font-medium text-slate-500">
      <span>{label}</span>
      <select
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="h-9 w-full rounded-md border border-input bg-white px-3 text-sm text-slate-700 outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
      >
        {children}
      </select>
    </label>
  )
}
