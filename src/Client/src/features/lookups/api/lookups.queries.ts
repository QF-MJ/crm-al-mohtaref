import { useQuery } from '@tanstack/react-query'
import { getLookups } from './lookups.api'

export function useLookups() {
  return useQuery({
    queryKey: ['lookups'],
    queryFn: ({ signal }) => getLookups(signal),
    staleTime: 5 * 60_000,
  })
}
