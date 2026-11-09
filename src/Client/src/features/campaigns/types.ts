export type CampaignStatus = 'draft' | 'active' | 'paused' | 'completed'

export interface CampaignQuestion {
  id: string
  text: string
  selected: boolean
}

export interface Campaign {
  id: number
  title: string
  description: string
  trackId: number
  trackName: string
  status: CampaignStatus
  platforms: string[]
  budget: number | null
  sheetUrl: string
  startDate: string
  endDate: string
  importedCustomers: number
  questions: CampaignQuestion[]
}
