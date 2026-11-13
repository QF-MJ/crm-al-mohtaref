import type { AppRoute } from '@/types/router'
import { UnavailableRoute } from '@/components/common/UnavailableRoute'

export const campaignsRoutes: AppRoute[] = [
  {
    path: 'campaigns',
    element: (
      <UnavailableRoute titleKey="campaigns.title" descriptionKey="campaigns.apiUnavailable" />
    ),
    handle: { capability: 'modules.preview' },
  },
  {
    path: 'campaigns/new',
    element: (
      <UnavailableRoute titleKey="campaigns.title" descriptionKey="campaigns.apiUnavailable" />
    ),
    handle: { capability: 'modules.preview' },
  },
  {
    path: 'campaigns/:campaignId/edit',
    element: (
      <UnavailableRoute titleKey="campaigns.title" descriptionKey="campaigns.apiUnavailable" />
    ),
    handle: { capability: 'modules.preview' },
  },
  {
    path: 'campaigns/reports',
    element: (
      <UnavailableRoute
        titleKey="campaigns.reportsTitle"
        descriptionKey="campaigns.reportsUnavailable"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
]
