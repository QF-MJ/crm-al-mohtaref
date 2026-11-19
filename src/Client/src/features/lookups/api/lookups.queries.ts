import { useQuery } from '@tanstack/react-query'
import type { LookupType } from '../types'
import { getLookups } from './lookups.api'

const userLookupTypes: LookupType[] = ['Departments', 'Roles']
const contactLookupTypes: LookupType[] = ['Tracks', 'LeadStatuses']
const emptyLookupTypes: LookupType[] = []
const campaignLookupTypes: LookupType[] = ['Tracks', 'CampaignStatuses', 'Platforms']
const campaignFallbackLookupTypes: LookupType[] = ['Tracks']

// include ثابتة لكل مجموعة حتى لا تتغير queryKey مع كل render
export function useLookups(include: LookupType[] = userLookupTypes, enabled = true) {
  const types = include.length > 0 ? include : emptyLookupTypes
  return useQuery({
    queryKey: ['lookups', types],
    queryFn: ({ signal }) => getLookups(types, signal),
    staleTime: 5 * 60_000,
    enabled: enabled && types.length > 0,
  })
}

// فلاتر العملاء: المسارات، حالات العملاء، والوسوم من نفس النداء.
// إذا نسخة الباك لا تدعم Tags بعد، نرجع للمسارات والحالات فقط بدل تعطيل الفلاتر.
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
          console.warn('[lookups] فشل طلب الوسوم، تم الرجوع للمسارات والحالات فقط.', error)
        }
        return await getLookups(contactLookupTypes, signal)
      }
    },
    staleTime: 5 * 60_000,
    retry: false,
    enabled,
  })
}

// فلاتر الحملات: المسارات، حالات الحملة، والمنصات من نفس النداء.
// الباك لا يدعم CampaignStatuses و Platforms بعد فيرفض الطلب كاملاً،
// عندها نرجع للمسارات فقط بدل تعطيل الصفحة.
export function useCampaignLookups(enabled = true) {
  return useQuery({
    queryKey: ['lookups', 'campaigns'],
    queryFn: async ({ signal }) => {
      try {
        return await getLookups(campaignLookupTypes, signal)
      } catch (error) {
        if (import.meta.env.DEV) {
          console.warn(
            '[lookups] الباك لا يدعم CampaignStatuses/Platforms بعد، تم الرجوع للمسارات فقط.',
            error,
          )
        }
        return await getLookups(campaignFallbackLookupTypes, signal)
      }
    },
    staleTime: 5 * 60_000,
    retry: false,
    enabled,
  })
}
