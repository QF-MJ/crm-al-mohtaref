import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { getCampaigns } from './campaigns.api'
import type { CampaignFilters } from '../types'

export function useCampaigns(filters: CampaignFilters) {
  return useQuery({
    queryKey: ['campaigns', filters],
    queryFn: ({ signal }) => getCampaigns(filters, signal),
    // نبقي الصفحة الحالية ظاهرة أثناء تحميل الصفحة التالية
    placeholderData: keepPreviousData,
  })
}
