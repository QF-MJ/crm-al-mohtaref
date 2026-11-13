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
