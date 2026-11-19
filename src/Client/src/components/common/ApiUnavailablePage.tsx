import { useTranslation } from 'react-i18next'
import { CircleSlash2, FileCode2 } from 'lucide-react'

interface ApiUnavailablePageProps {
  title: string
  description: string
  operation?: string
}

export function ApiUnavailablePage({ title, description, operation }: ApiUnavailablePageProps) {
  const { t } = useTranslation()

  return (
    <section
      className="mx-auto flex min-h-[420px] max-w-4xl flex-col items-center justify-center rounded-3xl border border-slate-200 bg-white px-6 py-12 text-center shadow-sm sm:px-12"
      role="note"
      aria-labelledby="api-unavailable-title"
    >
      <span className="mb-6 grid size-16 place-items-center rounded-2xl bg-amber-50 text-amber-700 ring-1 ring-inset ring-amber-200">
        <CircleSlash2 aria-hidden="true" className="size-7" />
      </span>
      <p className="mb-2 inline-flex items-center gap-2 text-xs font-semibold uppercase tracking-wide text-amber-700">
        <FileCode2 aria-hidden="true" className="size-4" />
        {t('apiUnavailable.eyebrow')}
      </p>
      <h1 id="api-unavailable-title" className="text-2xl font-bold text-slate-900">
        {title}
      </h1>
      <p className="mt-3 max-w-xl text-sm leading-7 text-slate-600">{description}</p>
      {operation && (
        <code dir="ltr" className="mt-5 rounded-lg bg-slate-100 px-3 py-2 text-xs text-slate-700">
          {operation}
        </code>
      )}
      <p className="mt-6 max-w-xl border-t border-slate-100 pt-5 text-xs leading-6 text-slate-500">
        {t('apiUnavailable.note')}
      </p>
    </section>
  )
}
