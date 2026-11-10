import { api } from '@/api/client'
import type { LookupsResponse } from '../types'

export function getLookups(signal?: AbortSignal) {
  return api
    .get<LookupsResponse>('/lookups', {
      params: { Include: ['Departments', 'Roles', 'Tracks', 'LeadStatuses'] },
      paramsSerializer: { indexes: null },
      signal,
    })
    .then((response) => response.data)
}
