import type { AppRoute } from '@/types/router'
import { ApiUnavailablePage } from '@/components/common/ApiUnavailablePage'
import { useTranslation } from 'react-i18next'

function AcademicUnavailablePage({
  titleKey,
  descriptionKey,
}: {
  titleKey: string
  descriptionKey: string
}) {
  const { t } = useTranslation()
  return <ApiUnavailablePage title={t(titleKey)} description={t(descriptionKey)} />
}

export const academicRoutes: AppRoute[] = [
  {
    path: 'academic/subscribed-clients',
    element: (
      <AcademicUnavailablePage
        titleKey="features.unavailable.academicSubscribedTitle"
        descriptionKey="features.unavailable.academicSubscribedDescription"
      />
    ),
    handle: { permission: 'academic.viewSubscribed' },
  },
  {
    path: 'academic/records/:studentId',
    element: (
      <AcademicUnavailablePage
        titleKey="features.unavailable.studentRecordTitle"
        descriptionKey="features.unavailable.studentRecordDescription"
      />
    ),
    handle: { permission: 'academic.records' },
  },
]
