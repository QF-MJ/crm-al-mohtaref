import { useQuery } from '@tanstack/react-query'
import { getContacts } from './contacts.api'
import type { ContactFilters } from '../types'

export function useContacts(filters: ContactFilters, enabled = true) {
  return useQuery({
    queryKey: ['contacts', filters],
    queryFn: ({ signal }) => getContacts(filters, signal),
    enabled,
  })
}
