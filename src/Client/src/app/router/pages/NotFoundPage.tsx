import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Compass } from 'lucide-react'
import { paths } from '../paths'

export function NotFoundPage() {
  const { t } = useTranslation()

  return (
    <main className="grid min-h-screen place-items-center bg-[#f5f7f8] p-6">
      <div className="max-w-md rounded-2xl border border-slate-200 bg-white px-6 py-10 text-center shadow-sm">
        <span className="mx-auto grid size-14 place-items-center rounded-2xl bg-slate-100 text-slate-500">
          <Compass aria-hidden="true" className="size-7" />
        </span>
        <h1 className="mt-4 text-lg font-semibold text-slate-900">{t('errors.notFound')}</h1>
        <p className="mt-2 text-sm leading-7 text-slate-500">{t('errors.notFoundHint')}</p>
        <Link
          to={paths.dashboard}
          className="mt-6 inline-flex h-9 items-center rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground hover:bg-primary/90"
        >
          {t('errors.backHome')}
        </Link>
      </div>
    </main>
  )
}
