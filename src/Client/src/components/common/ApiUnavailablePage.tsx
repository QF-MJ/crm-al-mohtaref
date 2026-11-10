import { useTranslation } from 'react-i18next'

interface ApiUnavailablePageProps {
  title: string
  description: string
}

export function ApiUnavailablePage({ title, description }: ApiUnavailablePageProps) {
  const { t } = useTranslation()

  return (
    <section className="mx-auto max-w-3xl space-y-3 rounded-xl border bg-white p-8 text-center" role="note">
      <h1 className="text-xl font-bold">{title}</h1>
      <p className="text-sm text-slate-600">{description}</p>
      <p className="text-xs text-slate-500">{t('apiUnavailable.note')}</p>
    </section>
  )
}
