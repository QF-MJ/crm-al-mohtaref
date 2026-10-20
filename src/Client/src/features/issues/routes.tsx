import type { AppRoute } from '@/types/router'
import { PagePlaceholder } from '@/components/common/PagePlaceholder'

export const issuesRoutes: AppRoute[] = [
  {
    path: 'issues',
    element: <PagePlaceholder name="Issues" fr="FR-23" />,
    handle: { permission: 'issues.follow' },
  },
  {
    path: 'issues/new',
    element: <PagePlaceholder name="IssueNew" fr="FR-21" />,
    handle: { permission: 'issues.create' },
  },
  {
    path: 'issues/:issueId',
    element: <PagePlaceholder name="IssueDetails" fr="FR-22 FR-23" />,
    handle: { permission: 'issues.follow' },
  },
]
