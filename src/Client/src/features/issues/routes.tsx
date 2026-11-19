import type { AppRoute } from '@/types/router'
import { ApiUnavailablePage } from '@/components/common/ApiUnavailablePage'
import { useTranslation } from 'react-i18next'

function IssueUnavailablePage({
  titleKey,
  descriptionKey,
}: {
  titleKey: string
  descriptionKey: string
}) {
  const { t } = useTranslation()
  return <ApiUnavailablePage title={t(titleKey)} description={t(descriptionKey)} />
}

export const issuesRoutes: AppRoute[] = [
  {
    path: 'issues',
    element: (
      <IssueUnavailablePage
        titleKey="features.unavailable.issuesTitle"
        descriptionKey="features.unavailable.issuesDescription"
      />
    ),
    handle: { permission: 'issues.follow' },
  },
  {
    path: 'issues/new',
    element: (
      <IssueUnavailablePage
        titleKey="features.unavailable.issueCreateTitle"
        descriptionKey="features.unavailable.issueCreateDescription"
      />
    ),
    handle: { permission: 'issues.create' },
  },
  {
    path: 'issues/:issueId',
    element: (
      <IssueUnavailablePage
        titleKey="features.unavailable.issueDetailsTitle"
        descriptionKey="features.unavailable.issueDetailsDescription"
      />
    ),
    handle: { permission: 'issues.follow' },
  },
]
