import { useQuery } from '@tanstack/react-query'
import type { LookupType } from '../types'
import { getLookups } from './lookups.api'

const departmentLookupTypes: LookupType[] = ['Departments']
const contactLookupTypes: LookupType[] = ['Tracks', 'Tags']
const emptyLookupTypes: LookupType[] = []

// include ثابتة لكل مجموعة حتى لا تتغير queryKey مع كل render
export function useLookups(include: LookupType[] = departmentLookupTypes, enabled = true) {
  const types = include.length > 0 ? include : emptyLookupTypes
  return useQuery({
    queryKey: ['lookups', types],
    queryFn: ({ signal }) => getLookups(types, signal),
    staleTime: 5 * 60_000,
    enabled: enabled && types.length > 0,
  })
}

// فلاتر العملاء: المسارات والوسوم من النداء.
// حالة العميل (LeadStatus) تعداد ثابت في الواجهة وليست من /lookups.
export function useContactLookups(enabled = true) {
  return useQuery({
    queryKey: ['lookups', 'contacts'],
    queryFn: ({ signal }) => getLookups(contactLookupTypes, signal),
    staleTime: 5 * 60_000,
    enabled,
  })
}
