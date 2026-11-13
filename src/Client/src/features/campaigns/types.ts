export type QuestionType = 1 | 2 | 3

export interface CampaignListItem {
  id: number
  title: string
  status?: number
  trackId?: number
}

export interface CampaignPage {
  items: CampaignListItem[]
  page: {
    pageNumber: number
    hasNextPage: boolean
  }
}

export interface CampaignQuestionAnswer {
  tagId: number
  answer: string
}

export interface CampaignQuestion {
  id: number
  question: string
  type: QuestionType | null
  fieldId?: number | null
  answers?: CampaignQuestionAnswer[]
  isSelected?: boolean
}

export interface CampaignDetail extends CampaignListItem {
  description?: string
  url?: string
  budget?: number
  platforms?: Array<{ id: number; name: string }>
  questions?: CampaignQuestion[]
}

export interface CampaignQuestions {
  campaignId: number
  questions: CampaignQuestion[]
}

export interface CampaignFilters {
  pageNumber: number
  title?: string
  status?: number
  trackId?: number
  platformId?: number
}

export interface UpdateCampaignRequest {
  title: string
  description?: string
  status: number
  trackId: number
  url?: string
  budget?: number
  platformIds?: number[]
  questionIds?: number[]
}

export interface UpdateQuestionRequest {
  question: string
  type: QuestionType
  fieldId: number | null
  answers: CampaignQuestionAnswer[]
}

export interface ImportResult {
  importedCount: number
  skippedCount: number
  errors?: Array<{ row?: number; message?: string }>
}
