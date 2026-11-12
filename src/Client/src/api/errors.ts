import axios from 'axios'
import type { ApiError } from '@/types/common'

// الباك يرجّع ErrorDto: { status, message } (camelCase بالرد الفعلي، والـ OpenAPI يكتبها بحرف كبير).
// نقبل الشكلين + title/errors كاحتياط لأخطاء ProblemDetails التي قد يرسلها ASP.NET قبل الـ middleware.
interface ErrorBody {
  status?: number
  message?: string
  Status?: number
  Message?: string
  title?: string
  errors?: Record<string, string[]>
}

export function toApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ErrorBody | undefined
    return {
      status: error.response?.status ?? data?.status ?? data?.Status ?? 0,
      message: data?.message ?? data?.Message ?? data?.title ?? error.message,
      fieldErrors: data?.errors,
    }
  }
  if (isApiError(error)) return error
  return { status: 0, message: error instanceof Error ? error.message : 'Unknown error' }
}

export function getApiErrorMessage(error: unknown, fallback: string) {
  const parsed = toApiError(error)
  return parsed.status === 0 || parsed.message === 'Unknown error' ? fallback : parsed.message
}

function isApiError(error: unknown): error is ApiError {
  return (
    typeof error === 'object' &&
    error !== null &&
    'status' in error &&
    typeof error.status === 'number' &&
    'message' in error &&
    typeof error.message === 'string'
  )
}
