import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { paths } from '../paths'

export function ForbiddenPage() {
  const { t } = useTranslation()
  return (
    <main className="grid min-h-screen place-items-center p-4 text-center">
      <div>
        <p className="mb-4 text-xl font-semibold">{t('errors.forbidden')}</p>
        <Link className="text-primary underline" to={paths.home}>
          {t('errors.backHome')}
        </Link>
      </div>
    </main>
  )
}
