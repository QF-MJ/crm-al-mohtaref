import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  getCampaign,
  getCampaignQuestions,
  getCampaigns,
  importCampaign,
  updateCampaign,
  updateQuestion,
} from './campaigns.api'
import type { CampaignFilters, UpdateCampaignRequest, UpdateQuestionRequest } from '../types'

const campaignsKey = ['campaigns'] as const

export function useCampaigns(filters: CampaignFilters) {
  return useQuery({
    queryKey: [...campaignsKey, filters],
    queryFn: ({ signal }) => getCampaigns(filters, signal),
  })
}

export function useCampaign(id: number | undefined) {
  return useQuery({
    queryKey: [...campaignsKey, 'detail', id],
    queryFn: ({ signal }) => {
      if (id === undefined) throw new Error('A campaign ID is required')
      return getCampaign(id, signal)
    },
    enabled: id !== undefined,
  })
}

export function useCampaignQuestions(id: number | undefined) {
  return useQuery({
    queryKey: [...campaignsKey, 'questions', id],
    queryFn: ({ signal }) => {
      if (id === undefined) throw new Error('A campaign ID is required')
      return getCampaignQuestions(id, signal)
    },
    enabled: id !== undefined,
  })
}

export function useUpdateCampaign(id: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: UpdateCampaignRequest) => updateCampaign(id, body),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: campaignsKey }),
        queryClient.invalidateQueries({ queryKey: [...campaignsKey, 'detail', id] }),
      ]),
  })
}

export function useImportCampaign(id: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => importCampaign(id),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: campaignsKey }),
        queryClient.invalidateQueries({ queryKey: [...campaignsKey, 'detail', id] }),
        queryClient.invalidateQueries({ queryKey: [...campaignsKey, 'questions', id] }),
      ]),
  })
}

export function useUpdateQuestion(campaignId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, body }: { id: number; body: UpdateQuestionRequest }) =>
      updateQuestion(id, body),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: [...campaignsKey, 'questions', campaignId] }),
        queryClient.invalidateQueries({ queryKey: [...campaignsKey, 'detail', campaignId] }),
      ]),
  })
}
