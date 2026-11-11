import { Megaphone } from 'lucide-react'
import { useTranslation } from 'react-i18next'

export function CampaignsUnavailablePage() {
  const { t } = useTranslation()

  return (
    <section
      className="mx-auto flex min-h-80 max-w-2xl flex-col items-center justify-center rounded-2xl border border-dashed bg-card p-8 text-center"
      aria-labelledby="campaigns-unavailable-title"
    >
      <span className="mb-5 flex size-14 items-center justify-center rounded-2xl bg-primary/10 text-primary">
        <Megaphone aria-hidden="true" className="size-7" />
      </span>
      <h1 id="campaigns-unavailable-title" className="text-2xl font-bold">
        {t('features.campaigns.title')}
      </h1>
      <p className="mt-3 max-w-lg text-sm leading-7 text-muted-foreground">
        {t('features.campaigns.description')}
      </p>
    </section>
  )
}
