import { AlertTriangle, RefreshCw } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { describeApiError } from '@/api/errors'
import { Button } from '@/components/ui/button'

interface ErrorStateProps {
  error: unknown
  onRetry?: () => void
  isRetrying?: boolean
}

// يجمع كل حالات الفشل: شبكة، صلاحية، أو رسالة الخادم — مع رسائل مفهومة دائماً
export function ErrorState({ error, onRetry, isRetrying }: ErrorStateProps) {
  const { t } = useTranslation()
  const text = describeApiError(error, t)

  return (
    <div role="alert" className="flex flex-col items-center gap-4 px-6 py-12 text-center">
      <span className="grid size-12 place-items-center rounded-2xl bg-rose-50 text-rose-600">
        <AlertTriangle aria-hidden="true" className="size-6" />
      </span>
      <p className="max-w-md text-sm leading-6 text-rose-800">{text}</p>
      {onRetry && (
        <Button type="button" variant="outline" size="sm" onClick={onRetry} disabled={isRetrying}>
          <RefreshCw aria-hidden="true" />
          {t('actions.retry')}
        </Button>
      )}
    </div>
  )
}
