import { useQuery } from '@tanstack/react-query'
import type { LookupType } from '../types'
import { getLookups } from './lookups.api'

const departmentLookupTypes: LookupType[] = ['Departments']
const contactLookupTypes: LookupType[] = ['Tracks']
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

// فلاتر العملاء: المسارات والوسوم من الـ API. حالات العميل أصبحت enum ثابت في الفرونت.
// إذا نسخة الباك لا تدعم Tags بعد، نرجع للمسارات فقط بدل تعطيل الفلاتر.
export function useContactLookups(enabled = true) {
  return useQuery({
    queryKey: ['lookups', 'contacts'],
    queryFn: async ({ signal }) => {
      try {
        const result = await getLookups([...contactLookupTypes, 'Tags'], signal)
        if (import.meta.env.DEV && !result.tags?.length) {
          console.warn(
            '[lookups] الرد لا يحتوي وسوم. تأكد أن الباك يضيف Tags إلى LookupType و LookupsDto و GetLookupsQueryHandler.',
            result,
          )
        }
        return result
      } catch (error) {
        if (import.meta.env.DEV) {
          console.warn('[lookups] فشل طلب الوسوم، تم الرجوع للمسارات فقط.', error)
        }
        return await getLookups(contactLookupTypes, signal)
      }
    },
    staleTime: 5 * 60_000,
    retry: false,
    enabled,
  })
}
