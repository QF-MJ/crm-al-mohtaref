import axios from 'axios'
import type { ApiError } from '@/types/common'

export function toApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { status?: number; message?: string } | undefined
    return {
      status: error.response?.status ?? data?.status ?? 0,
      message: data?.message ?? error.message,
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
