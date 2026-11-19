import type { AppRoute } from '@/types/router'
import { UnavailableRoute } from '@/components/common/UnavailableRoute'
import { CampaignsPage } from './pages/CampaignsPage'

export const campaignsRoutes: AppRoute[] = [
  {
    path: 'campaigns',
    element: <CampaignsPage />,
    handle: { capability: 'campaigns.view' },
  },
]

// صفحات لم تُبنَ بعد: تُسجّل في وضع التطوير فقط (انظر app/router/index.tsx)
export const campaignsPreviewRoutes: AppRoute[] = [
  {
    path: 'campaigns/new',
    element: (
      <UnavailableRoute titleKey="campaigns.createTitle" descriptionKey="campaigns.notReady" />
    ),
    handle: { capability: 'campaigns.create' },
  },
  {
    path: 'campaigns/:campaignId',
    element: (
      <UnavailableRoute titleKey="campaigns.detailsTitle" descriptionKey="campaigns.notReady" />
    ),
    handle: { capability: 'campaigns.view' },
  },
]
