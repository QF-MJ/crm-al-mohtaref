import type { AppRoute } from '@/types/router'
import { ComingSoonPage } from '@/components/common/ComingSoonPage'

export const campaignsRoutes: AppRoute[] = [
  {
    path: 'campaigns',
    element: <ComingSoonPage titleKey="nav.campaigns" />,
    handle: { capability: 'campaigns.view' },
  },
]
