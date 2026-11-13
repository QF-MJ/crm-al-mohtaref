import { AlertTriangle, RefreshCw } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { toApiError } from '@/api/errors'
import { Button } from '@/components/ui/button'

interface ErrorStateProps {
  error: unknown
  onRetry?: () => void
  isRetrying?: boolean
  /** رسالة بديلة عندما لا يرسل الخادم نصاً مفيداً */
  fallback?: string
}

// يجمع كل حالات الفشل في مكان واحد: شبكة، صلاحية، او رسالة الخادم
export function ErrorState({ error, onRetry, isRetrying, fallback }: ErrorStateProps) {
  const { t } = useTranslation()
  const { status, message } = toApiError(error)

  const text =
    status === 0
      ? t('errors.connection')
      : status === 403
        ? t('errors.forbiddenAction')
        : status >= 500
          ? t('errors.server')
          : message && message !== 'Unknown error'
            ? message
            : (fallback ?? t('errors.requestFailed'))

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
