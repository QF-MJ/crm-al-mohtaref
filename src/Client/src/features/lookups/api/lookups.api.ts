import { api } from '@/api/client'
import type { LookupType, LookupsResponse } from '../types'

export function getLookups(include: LookupType[], signal?: AbortSignal) {
  return api
    .get<LookupsResponse>('/lookups', {
      params: { include },
      paramsSerializer: { indexes: null },
      signal,
    })
    .then((response) => response.data)
}
