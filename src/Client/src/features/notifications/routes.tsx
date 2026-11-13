import type { AppRoute } from '@/types/router'
import { UnavailableRoute } from '@/components/common/UnavailableRoute'

export const notificationsRoutes: AppRoute[] = [
  {
    path: 'notifications',
    element: (
      <UnavailableRoute
        titleKey="features.unavailable.notificationsTitle"
        descriptionKey="features.unavailable.notificationsDescription"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
]
