import { api } from '@/api/client'
import type { LookupItem, LookupType, LookupsResponse } from '../types'

// نتحمّل اختلاف أسماء الحقول القادمة من الباك (camelCase/PascalCase/أسماء بديلة)
function firstArray(source: Record<string, unknown>, keys: string[]): LookupItem[] | undefined {
  for (const key of keys) {
    const value = source[key]
    if (Array.isArray(value)) return value as LookupItem[]
  }
  return undefined
}

function normalizeLookups(data: LookupsResponse): LookupsResponse {
  const raw = data as Record<string, unknown>
  return {
    departments: data.departments ?? firstArray(raw, ['Departments']),
    roles: data.roles ?? firstArray(raw, ['Roles']),
    tracks: data.tracks ?? firstArray(raw, ['Tracks']),
    leadStatuses: data.leadStatuses ?? firstArray(raw, ['LeadStatuses']),
    tags: data.tags ?? firstArray(raw, ['Tags', 'tagList', 'tagsList']),
  }
}

export function getLookups(include: LookupType[], signal?: AbortSignal) {
  return api
    .get<LookupsResponse>('/lookups', {
      // لا نرسل include فارغاً: الباك يفشل عندها، والقيمة الافتراضية عنده غير معرّفة
      params: include.length > 0 ? { include } : undefined,
      paramsSerializer: { indexes: null },
      signal,
    })
    .then((response) => normalizeLookups(response.data))
}
