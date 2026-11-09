import type { AppRoute } from '@/types/router'
import { PagePlaceholder } from '@/components/common/PagePlaceholder'
import { CampaignsPage } from './pages/CampaignsPage'
import { CampaignFormPage } from './pages/CampaignFormPage'

export const campaignsRoutes: AppRoute[] = [
  {
    path: 'campaigns',
    element: <CampaignsPage />,
    handle: { permission: 'campaigns.updateStatus' },
  },
  {
    path: 'campaigns/new',
    element: <CampaignFormPage />,
    handle: { permission: 'campaigns.create' },
  },
  {
    path: 'campaigns/:campaignId/edit',
    element: <CampaignFormPage />,
    handle: { permission: 'campaigns.updateStatus' },
  },
  {
    path: 'campaigns/reports',
    element: <PagePlaceholder name="CampaignReports" fr="FR-15 to FR-17" />,
    handle: { permission: 'campaigns.reports' },
  },
]
