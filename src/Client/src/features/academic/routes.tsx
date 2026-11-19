import type { AppRoute } from '@/types/router'
import { UnavailableRoute } from '@/components/common/UnavailableRoute'

export const academicRoutes: AppRoute[] = [
  {
    path: 'academic/subscribed-clients',
    element: (
      <UnavailableRoute
        titleKey="features.unavailable.academicSubscribedTitle"
        descriptionKey="features.unavailable.academicSubscribedDescription"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
  {
    path: 'academic/records/:studentId',
    element: (
      <UnavailableRoute
        titleKey="features.unavailable.studentRecordTitle"
        descriptionKey="features.unavailable.studentRecordDescription"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
]
