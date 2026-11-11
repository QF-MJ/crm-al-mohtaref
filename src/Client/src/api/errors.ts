import axios from 'axios'
import type { ApiError } from '@/types/common'

export function toApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { status?: number; message?: string } | undefined
    const status = error.response?.status ?? data?.status ?? 0
    return {
      status,
      message: data?.message ?? fallbackMessage(status, error.message),
    }
  }
  if (isApiError(error)) return error
  return { status: 0, message: error instanceof Error ? error.message : 'Unknown error' }
}

function fallbackMessage(status: number, fallback: string) {
  if (status === 401) return 'انتهت الجلسة، سجّل دخولك من جديد'
  if (status === 422) return 'محاولات كثيرة، انتظر دقيقة'
  if (status === 403) return 'لا تملك صلاحية الوصول لهذه العملية'
  return status >= 500 ? 'حدث خطأ غير متوقع' : fallback
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
