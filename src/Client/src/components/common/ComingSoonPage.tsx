import { Clock3 } from 'lucide-react'
import { useTranslation } from 'react-i18next'

// صفحة موحّدة للأقسام التي ستُضاف لاحقاً: رسالة واحدة واضحة بلا تفاصيل تقنية
export function ComingSoonPage({ titleKey }: { titleKey: string }) {
  const { t } = useTranslation()

  return (
    <section className="grid min-h-[55vh] place-items-center">
      <div className="w-full max-w-md rounded-2xl border border-slate-200 bg-white px-6 py-12 text-center shadow-sm sm:px-10">
        <span className="mx-auto grid size-12 place-items-center rounded-xl bg-teal-50 text-teal-700">
          <Clock3 aria-hidden="true" className="size-6" />
        </span>
        <h1 className="mt-4 text-lg font-semibold text-slate-900">{t(titleKey)}</h1>
        <p className="mt-2 text-sm leading-7 text-slate-500">{t('comingSoon.description')}</p>
      </div>
    </section>
  )
}
