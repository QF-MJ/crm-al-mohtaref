import type { AppRoute } from '@/types/router'
<<<<<<< Updated upstream
import { ApiUnavailablePage } from '@/components/common/ApiUnavailablePage'
import { CampaignCreateUnavailablePage } from './pages/CampaignFormPage'
import { useTranslation } from 'react-i18next'

function CampaignsUnavailablePage() {
  const { t } = useTranslation()
  return <ApiUnavailablePage title={t('campaigns.title')} description={t('apiUnavailable.note')} />
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
=======
import { CampaignsUnavailablePage } from './pages/CampaignsUnavailablePage'
>>>>>>> Stashed changes

export const campaignsRoutes: AppRoute[] = [
  {
    path: 'campaigns',
    element: <CampaignsUnavailablePage />,
    handle: { permission: 'campaigns.updateStatus' },
  },
  {
    path: 'campaigns/new',
<<<<<<< Updated upstream
    element: <CampaignCreateUnavailablePage />,
=======
    element: <CampaignsUnavailablePage />,
>>>>>>> Stashed changes
    handle: { permission: 'campaigns.create' },
  },
  {
    path: 'campaigns/:campaignId/edit',
    element: <CampaignsUnavailablePage />,
    handle: { permission: 'campaigns.updateStatus' },
  },
  {
    path: 'campaigns/reports',
<<<<<<< Updated upstream
    element: <CampaignReportsUnavailablePage />,
=======
    element: <CampaignsUnavailablePage />,
>>>>>>> Stashed changes
    handle: { permission: 'campaigns.reports' },
  },
]
