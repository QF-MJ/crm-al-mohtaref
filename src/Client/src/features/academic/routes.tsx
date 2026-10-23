import type { AppRoute } from '@/types/router'
import { PagePlaceholder } from '@/components/common/PagePlaceholder'

export const academicRoutes: AppRoute[] = [
  {
    path: 'academic/subscribed-clients',
    element: <PagePlaceholder name="SubscribedClients" fr="FR-18 FR-19" />,
    handle: { permission: 'academic.viewSubscribed' },
  },
  {
    path: 'academic/records/:studentId',
    element: <PagePlaceholder name="StudentRecord" fr="FR-20" />,
    handle: { permission: 'academic.records' },
  },
]
