import type { AppRoute } from '@/types/router'
import { ApiUnavailablePage } from '@/components/common/ApiUnavailablePage'
import { useTranslation } from 'react-i18next'

function CampaignsUnavailablePage() {
  const { t } = useTranslation()
  return (
    <ApiUnavailablePage
      title={t('campaigns.title')}
      description={t('campaigns.apiUnavailable')}
    />
  )
}

function CampaignReportsUnavailablePage() {
  const { t } = useTranslation()
  return (
    <ApiUnavailablePage
      title={t('campaigns.reportsTitle')}
      description={t('campaigns.reportsUnavailable')}
    />
  )
}

export const campaignsRoutes: AppRoute[] = [
  {
    path: 'campaigns',
    element: <CampaignsUnavailablePage />,
    handle: { permission: 'campaigns.updateStatus' },
  },
  {
    path: 'campaigns/new',
    element: <CampaignsUnavailablePage />,
    handle: { permission: 'campaigns.create' },
  },
  {
    path: 'campaigns/:campaignId/edit',
    element: <CampaignsUnavailablePage />,
    handle: { permission: 'campaigns.updateStatus' },
  },
  {
    path: 'campaigns/reports',
    element: <CampaignReportsUnavailablePage />,
    handle: { permission: 'campaigns.reports' },
  },
]
