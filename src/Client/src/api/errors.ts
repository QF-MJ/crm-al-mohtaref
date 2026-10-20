import axios from 'axios'
import type { ApiError } from '@/types/common'

// حوّل أي خطأ لشكل واحد. عدّل الحقول حسب شكل أخطاء الباك
// (ASP.NET Core ProblemDetails يرسل title و errors)
export function toApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as
      | { message?: string; title?: string; errors?: Record<string, string[]> }
      | undefined
    return {
      status: error.response?.status ?? 0,
      message: data?.message ?? data?.title ?? error.message,
      fieldErrors: data?.errors,
    }
  }
  return { status: 0, message: error instanceof Error ? error.message : 'Unknown error' }
}
