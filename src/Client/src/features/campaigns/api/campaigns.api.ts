import { api } from '@/api/client'
import type { CampaignFilters, CampaignListItem, CampaignsPage } from '../types'

// GET /api/campaigns → { items: [{ id, title, statusId, trackId }], page: { pageNumber, hasNextPage } }
export function getCampaigns(filters: CampaignFilters, signal?: AbortSignal) {
  return api
    .get<CampaignsPage>('/campaigns', {
      params: {
        pageNumber: filters.pageNumber,
        title: filters.title,
        status: filters.status,
        trackId: filters.trackId,
        platformId: filters.platformId,
      },
      signal,
    })
    .then((response) => normalizePage(response.data))
}

function normalizePage(data: CampaignsPage): CampaignsPage {
  return {
    items: Array.isArray(data?.items) ? data.items.map(normalizeCampaign) : [],
    page: data?.page,
  }
}

function normalizeCampaign(campaign: CampaignListItem): CampaignListItem {
  return {
    id: String(campaign?.id ?? ''),
    title: campaign?.title ?? '',
    statusId: String(campaign?.statusId ?? ''),
    trackId: String(campaign?.trackId ?? ''),
  }
}
