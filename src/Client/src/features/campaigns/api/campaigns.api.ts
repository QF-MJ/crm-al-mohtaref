import { api } from '@/api/client'
import type {
  CampaignDetail,
  CampaignFilters,
  CampaignPage,
  CampaignQuestions,
  ImportResult,
  UpdateCampaignRequest,
  UpdateQuestionRequest,
} from '../types'

export function getCampaigns(filters: CampaignFilters, signal?: AbortSignal) {
  return api
    .get<CampaignPage>('/campaigns', {
      params: {
        PageNumber: filters.pageNumber,
        Title: filters.title,
        Status: filters.status,
        TrackId: filters.trackId,
        PlatformId: filters.platformId,
      },
      signal,
    })
    .then((response) => response.data)
}

export function getCampaign(id: number, signal?: AbortSignal) {
  return api.get<CampaignDetail>(`/campaigns/${id}`, { signal }).then((response) => response.data)
}

export function updateCampaign(id: number, body: UpdateCampaignRequest) {
  return api.put<void>(`/campaigns/${id}`, body).then((response) => response.data)
}

export function getCampaignQuestions(id: number, signal?: AbortSignal) {
  return api
    .get<CampaignQuestions>(`/campaigns/${id}/questions`, { signal })
    .then((response) => response.data)
}

export function importCampaign(id: number) {
  return api.post<ImportResult>(`/campaigns/${id}/import`).then((response) => response.data)
}

export function updateQuestion(id: number, body: UpdateQuestionRequest) {
  return api.put<void>(`/questions/${id}`, body).then((response) => response.data)
}
