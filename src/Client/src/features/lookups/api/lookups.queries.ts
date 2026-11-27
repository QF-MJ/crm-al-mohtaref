import { useQuery } from '@tanstack/react-query'
import type { LookupType } from '../types'
import { getLookups } from './lookups.api'

const departmentLookupTypes: LookupType[] = ['Departments']
const contactLookupTypes: LookupType[] = ['Tracks', 'Tags']
const emptyLookupTypes: LookupType[] = []
const campaignLookupTypes: LookupType[] = ['Tracks', 'CampaignStatuses', 'Platforms']
const campaignFallbackLookupTypes: LookupType[] = ['Tracks']

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
