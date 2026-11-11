import { useTranslation } from 'react-i18next'
import { ApiUnavailablePage } from '@/components/common/ApiUnavailablePage'

export function CreateUserPage() {
  const { t } = useTranslation()
  return (
    <ApiUnavailablePage
      title={t('users.createTitle')}
      description={t('users.createUnavailable')}
      operation="POST /api/users"
    />
  )
}
