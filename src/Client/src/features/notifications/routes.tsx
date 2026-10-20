import type { AppRoute } from '@/types/router'
import { PagePlaceholder } from '@/components/common/PagePlaceholder'

export const notificationsRoutes: AppRoute[] = [
  {
    path: 'notifications',
    element: <PagePlaceholder name="Notifications" fr="FR-03" />,
    handle: { permission: 'notifications.view' },
  },
]
