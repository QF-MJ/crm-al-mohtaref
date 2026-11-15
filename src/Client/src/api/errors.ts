import axios from 'axios'
import type { ApiError } from '@/types/common'

// الباك يرجّع ErrorDto: { status, message } (camelCase). نقبل أيضاً ProblemDetails
// التي قد يرسلها ASP.NET قبل الـ middleware، وأي جسم نصي.
interface ErrorBody {
  status?: number
  message?: string
  Status?: number
  Message?: string
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

function parseBody(data: unknown): ErrorBody | undefined {
  if (!data) return undefined
  if (typeof data === 'string') return { message: data }
  if (typeof data === 'object') return data as ErrorBody
  return undefined
}

export function toApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    const body = parseBody(error.response?.data)
    const fieldErrors = body?.errors
    const firstFieldError = fieldErrors
      ? Object.values(fieldErrors)
          .flat()
          .find((value) => Boolean(value))
      : undefined

    return {
      status: error.response?.status ?? body?.status ?? body?.Status ?? 0,
      message:
        body?.message ??
        body?.Message ??
        body?.title ??
        firstFieldError ??
        body?.detail ??
        error.message,
      fieldErrors,
    }
  }

  if (isApiError(error)) return error
  return { status: 0, message: error instanceof Error ? error.message : 'Unknown error' }
}

export function isApiError(error: unknown): error is ApiError {
  return (
    typeof error === 'object' &&
    error !== null &&
    'status' in error &&
    typeof error.status === 'number' &&
    'message' in error &&
    typeof error.message === 'string'
  )
}

// رسائل Axios/الشبكة التقنية لا تُعرض للمستخدم النهائي
const TECHNICAL_MESSAGE_PATTERNS = [
  /^request failed with status code \d+$/i,
  /^network error$/i,
  /^timeout of \d+ms exceeded$/i,
  /^unknown error$/i,
  /^canceled$/i,
  /^failed to fetch$/i,
]

export function isHumanMessage(value: unknown): value is string {
  if (typeof value !== 'string') return false
  const message = value.trim()
  if (!message || message.length > 200) return false
  // صفحات HTML التي قد يرجّعها proxy/الخادم عند 502 وما شابه
  if (/[<>]/.test(message)) return false
  return !TECHNICAL_MESSAGE_PATTERNS.some((pattern) => pattern.test(message))
}

type Translate = (key: string) => string

/**
 * يحوّل أي خطأ إلى جملة مفهومة للمستخدم:
 * - يستخدم رسالة الخادم فقط إذا كانت جملة بشرية (عربية/مفيدة).
 * - وإلا يعرض رسالة عامة مناسبة لنوع الخطأ والحالة.
 */
export function describeApiError(
  error: unknown,
  t: Translate,
  fallbackKey = 'errors.requestFailed',
): string {
  const { status, message } = toApiError(error)
  const human = isHumanMessage(message) ? message.trim() : undefined

  // 502/503/504 غالباً تعني أن الخادم غير متاح (proxy أو خدمة متوقفة)
  if (status === 502 || status === 503 || status === 504) {
    return human ?? t('errors.unavailable')
  }
  if (status === 0) return t('errors.connection')
  if (status >= 500) return t('errors.server')
  if (status === 401) return human ?? t('errors.invalidCredentials')
  if (status === 403) return human ?? t('errors.forbiddenAction')
  if (status === 404) return human ?? t('errors.notFoundItem')
  if (status === 409) return human ?? t('errors.conflict')
  if (status === 422) return human ?? t('errors.invalidRequest')
  if (status === 429) return human ?? t('errors.tooManyAttempts')
  if (status === 400) return human ?? t('errors.invalidRequest')

  return human ?? t(fallbackKey)
}
