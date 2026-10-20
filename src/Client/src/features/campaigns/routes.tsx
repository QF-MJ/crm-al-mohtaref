import type { AppRoute } from '@/types/router'
import { PagePlaceholder } from '@/components/common/PagePlaceholder'

export const campaignsRoutes: AppRoute[] = [
  {
    path: 'campaigns',
    element: <PagePlaceholder name="Campaigns" fr="FR-14" />,
    handle: { permission: 'campaigns.updateStatus' },
  },
  {
    path: 'campaigns/new',
    element: <PagePlaceholder name="CampaignNew" fr="FR-13" />,
    handle: { permission: 'campaigns.create' },
  },
  {
    path: 'campaigns/reports',
    element: <PagePlaceholder name="CampaignReports" fr="FR-15 to FR-17" />,
    handle: { permission: 'campaigns.reports' },
  },
]
