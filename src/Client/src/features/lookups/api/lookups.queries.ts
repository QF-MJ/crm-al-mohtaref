import { useQuery } from '@tanstack/react-query'
import type { LookupType } from '../types'
import { getLookups } from './lookups.api'

const campaignLookupTypes: LookupType[] = ['Tracks', 'CampaignStatuses', 'Platforms']
const campaignFallbackLookupTypes: LookupType[] = ['Tracks']
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

// فلاتر العملاء: نحاول جلب المسارات والحملات والوسوم، مع تراجع تدريجي
// لأن بعض نسخ الباك لا تدعم Campaigns أو Tags في /lookups بعد.
export function useContactLookups(enabled = true) {
  return useQuery({
    queryKey: ['lookups', 'contacts'],
    queryFn: async ({ signal }) => {
      const attempts: LookupType[][] = [
        ['Tracks', 'Campaigns', 'Tags'],
        ['Tracks', 'Tags'],
        ['Tracks', 'Campaigns'],
        ['Tracks'],
      ]

      let lastError: unknown
      for (const types of attempts) {
        try {
          const result = await getLookups(types, signal)
          if (import.meta.env.DEV && types.includes('Tags') && !result.tags?.length) {
            console.warn(
              '[lookups] الرد لا يحتوي وسوم. تأكد أن الباك يضيف Tags إلى LookupType و LookupsDto و GetLookupsQueryHandler.',
              result,
            )
          }
          return result
        } catch (error) {
          lastError = error
        }
      }

      throw lastError
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
    enabled,
  })
}
