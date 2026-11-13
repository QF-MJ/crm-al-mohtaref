import { ArrowLeft, ArrowRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'

interface PaginationProps {
  pageNumber: number
  hasNextPage: boolean
  onPrevious: () => void
  onNext: () => void
  isFetching?: boolean
}

// الباك لا يرسل عدد الصفحات ولا الإجمالي، لذلك التنقل سابق/تالي فقط
export function Pagination({
  pageNumber,
  hasNextPage,
  onPrevious,
  onNext,
  isFetching = false,
}: PaginationProps) {
  const { t } = useTranslation()

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 px-4 py-3 sm:px-5">
      <span className="text-xs text-slate-500">{t('pagination.page', { page: pageNumber })}</span>
      <div className="flex gap-2">
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={pageNumber <= 1 || isFetching}
          onClick={onPrevious}
        >
          <ArrowRight aria-hidden="true" />
          {t('pagination.previous')}
        </Button>
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={!hasNextPage || isFetching}
          onClick={onNext}
        >
          {t('pagination.next')}
          <ArrowLeft aria-hidden="true" />
        </Button>
      </div>
    </div>
  )
}
