import { api } from '@/api/client'
import type { LookupType, LookupsResponse } from '../types'

export function getLookups(include: LookupType[], signal?: AbortSignal) {
  return api
    .get<LookupsResponse>('/lookups', {
      // لا نرسل include فارغاً: الباك يفشل عندها، والقيمة الافتراضية عنده غير معرّفة
      params: include.length > 0 ? { include } : undefined,
      paramsSerializer: { indexes: null },
      signal,
    })
    .then((response) => response.data)
}
