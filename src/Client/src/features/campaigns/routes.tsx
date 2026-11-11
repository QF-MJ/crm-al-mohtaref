import type { AppRoute } from '@/types/router'
import { ApiUnavailablePage } from '@/components/common/ApiUnavailablePage'
import { useTranslation } from 'react-i18next'
import { CampaignCreateUnavailablePage, CampaignFormPage } from './pages/CampaignFormPage'
import { CampaignsPage } from './pages/CampaignsPage'

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
    element: <CampaignsPage />,
    handle: { permission: 'campaigns.updateStatus' },
  },
  {
    path: 'campaigns/new',
    element: <CampaignCreateUnavailablePage />,
    handle: { permission: 'campaigns.create' },
  },
  {
    path: 'campaigns/:campaignId/edit',
    element: <CampaignFormPage />,
    handle: { permission: 'campaigns.updateStatus' },
  },
  {
    path: 'campaigns/reports',
    element: <CampaignReportsUnavailablePage />,
    handle: { permission: 'campaigns.reports' },
  },
]
