import { api } from '@/api/client'
import type { LookupsResponse } from '../types'

export function getLookups(signal?: AbortSignal) {
  return api
    .get<LookupsResponse>('/lookups', { signal })
    .then((response) => response.data)
}
