import type { AppRoute } from '@/types/router'
import { UnavailableRoute } from '@/components/common/UnavailableRoute'

export const issuesRoutes: AppRoute[] = [
  {
    path: 'issues',
    element: (
      <UnavailableRoute
        titleKey="features.unavailable.issuesTitle"
        descriptionKey="features.unavailable.issuesDescription"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
  {
    path: 'issues/new',
    element: (
      <UnavailableRoute
        titleKey="features.unavailable.issueCreateTitle"
        descriptionKey="features.unavailable.issueCreateDescription"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
  {
    path: 'issues/:issueId',
    element: (
      <UnavailableRoute
        titleKey="features.unavailable.issueDetailsTitle"
        descriptionKey="features.unavailable.issueDetailsDescription"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
]
