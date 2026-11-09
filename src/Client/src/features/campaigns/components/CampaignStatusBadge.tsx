import { useTranslation } from 'react-i18next'
import type { CampaignStatus } from '../types'

interface CampaignStatusBadgeProps {
  status: CampaignStatus
}

const statusStyles: Record<CampaignStatus, string> = {
  draft: 'bg-amber-50 text-amber-800',
  active: 'bg-teal-50 text-teal-800',
  paused: 'bg-slate-100 text-slate-700',
  completed: 'bg-blue-50 text-blue-800',
}

export function CampaignStatusBadge({ status }: CampaignStatusBadgeProps) {
  const { t } = useTranslation()

  return (
    <span className={`rounded-full px-3 py-1 text-xs font-medium ${statusStyles[status]}`}>
      {t(`campaigns.statuses.${status}`)}
    </span>
  )
}
