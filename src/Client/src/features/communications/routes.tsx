import type { AppRoute } from '@/types/router'
import { ApiUnavailablePage } from '@/components/common/ApiUnavailablePage'
import { useTranslation } from 'react-i18next'

function CommunicationsUnavailablePage() {
  const { t } = useTranslation()
  return (
    <ApiUnavailablePage
      title={t('features.unavailable.communicationsTitle')}
      description={t('features.unavailable.communicationsDescription')}
    />
  )
}

export const communicationsRoutes: AppRoute[] = [
  {
    path: 'clients/:clientId/communications',
    element: <CommunicationsUnavailablePage />,
    handle: { permission: 'communications.manage' },
  },
]
