import type { AppRoute } from '@/types/router'
import { UnavailableRoute } from '@/components/common/UnavailableRoute'

export const communicationsRoutes: AppRoute[] = [
  {
    path: 'clients/:clientId/communications',
    element: (
      <UnavailableRoute
        titleKey="features.unavailable.communicationsTitle"
        descriptionKey="features.unavailable.communicationsDescription"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
]
