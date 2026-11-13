import type { AppRoute } from '@/types/router'
import { ApiUnavailablePage } from '@/components/common/ApiUnavailablePage'
import { useTranslation } from 'react-i18next'

function ChangePasswordUnavailablePage() {
  const { t } = useTranslation()
  return (
    <ApiUnavailablePage
      title={t('features.unavailable.passwordTitle')}
      description={t('features.unavailable.passwordDescription')}
    />
  )
}

export const authRoutes: AppRoute[] = [
  {
    path: 'account/password',
    element: <ChangePasswordUnavailablePage />,
    handle: { permission: 'account.changePassword' },
  },
]
