import { useQuery } from '@tanstack/react-query'
import type { LookupType } from '../types'
import { getLookups } from './lookups.api'

const departmentLookupTypes: LookupType[] = ['Departments']
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

// فلاتر العملاء: المسارات والحملات أساسية، مع محاولة جلب الوسوم اختيارياً
// للتوافق مع اختلاف دعم الأنواع بين نسخ الباك. حالات العميل enum ثابت في الفرونت.
export function useContactLookups(enabled = true) {
  return useQuery({
    queryKey: ['lookups', 'contacts'],
    queryFn: async ({ signal }) => {
      const baseTypes: LookupType[] = ['Tracks', 'Campaigns']
      try {
        const result = await getLookups([...baseTypes, 'Tags'], signal)
        if (import.meta.env.DEV && !result.tags?.length) {
          console.warn(
            '[lookups] الرد لا يحتوي وسوم. تأكد أن الباك يضيف Tags إلى LookupType و LookupsDto و GetLookupsQueryHandler.',
            result,
          )
        }
        return result
      } catch (error) {
        if (import.meta.env.DEV) {
          console.warn('[lookups] فشل طلب الوسوم، سيتم المحاولة بدونها.', error)
        }
      }
      return await getLookups(baseTypes, signal)
    },
    staleTime: 5 * 60_000,
    retry: false,
    enabled,
  })
}
