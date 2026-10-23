import type { AppRoute } from '@/types/router'
import { PagePlaceholder } from '@/components/common/PagePlaceholder'

export const communicationsRoutes: AppRoute[] = [
  {
    path: 'clients/:clientId/communications',
    element: <PagePlaceholder name="ClientCommunications" fr="FR-11 FR-12" />,
    handle: { permission: 'communications.manage' },
  },
]
