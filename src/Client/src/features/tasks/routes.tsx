import type { AppRoute } from '@/types/router'
import { ComingSoonPage } from '@/components/common/ComingSoonPage'

export const tasksRoutes: AppRoute[] = [
  {
    path: 'tasks',
    element: <ComingSoonPage titleKey="nav.tasks" />,
    handle: { capability: 'tasks.view' },
  },
]
