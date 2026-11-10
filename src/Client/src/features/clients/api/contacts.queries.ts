import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getContacts } from './contacts.api'
import { useLookups } from '@/features/lookups/api/lookups.queries'
import type { ContactFilters } from '../types'

export function useContacts(filters: ContactFilters) {
  return useQuery({
    queryKey: ['contacts', filters],
    queryFn: ({ signal }) => getContacts(filters, signal),
  })
}

export function useContactLookups() {
  const query = useLookups()
  const data = useMemo(
    () => query.data
      ? {
          tracks: query.data.tracks ?? [],
          leadStatuses: query.data.leadStatuses ?? [],
          tags: query.data.tags,
        }
      : undefined,
    [query.data],
  )
  return {
    ...query,
    data,
  }
}
