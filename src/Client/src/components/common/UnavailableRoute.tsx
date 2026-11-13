import { useTranslation } from 'react-i18next'
import { ApiUnavailablePage } from './ApiUnavailablePage'

interface UnavailableRouteProps {
  titleKey: string
  descriptionKey: string
}

// صفحة موحّدة للوحدات التي لم تُطلق في الـAPI بعد، بمفاتيح ترجمة فقط
export function UnavailableRoute({ titleKey, descriptionKey }: UnavailableRouteProps) {
  const { t } = useTranslation()
  return <ApiUnavailablePage title={t(titleKey)} description={t(descriptionKey)} />
}
