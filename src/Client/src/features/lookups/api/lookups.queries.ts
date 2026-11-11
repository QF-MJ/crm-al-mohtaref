import { useQuery } from '@tanstack/react-query'
import type { LookupType } from '../types'
import { getLookups } from './lookups.api'

const userLookupTypes: LookupType[] = ['Departments', 'Roles']

export function useLookups(include: LookupType[] = userLookupTypes) {
  return useQuery({
    queryKey: ['lookups', include],
    queryFn: ({ signal }) => getLookups(include, signal),
    staleTime: 5 * 60_000,
  })
}
