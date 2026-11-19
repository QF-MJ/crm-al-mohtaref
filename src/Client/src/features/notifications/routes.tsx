import type { AppRoute } from '@/types/router'
import { ApiUnavailablePage } from '@/components/common/ApiUnavailablePage'
import { useTranslation } from 'react-i18next'

function NotificationsUnavailablePage() {
  const { t } = useTranslation()
  return (
    <ApiUnavailablePage
      title={t('features.unavailable.notificationsTitle')}
      description={t('features.unavailable.notificationsDescription')}
    />
  )
}

export const notificationsRoutes: AppRoute[] = [
  {
    path: 'notifications',
    element: <NotificationsUnavailablePage />,
    handle: { permission: 'notifications.view' },
  },
]
