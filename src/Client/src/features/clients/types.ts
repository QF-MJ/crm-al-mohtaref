// TODO: Replace this open shape after the verified ContactsPageDto is available.
export type Contact = Record<string, unknown>

// UNVERIFIED: only the likely items/page envelope is adapted; preserve other response data.
export interface ContactsPage {
  items: Contact[]
  page?: {
    pageNumber: number
    hasNextPage: boolean
  }
}

export interface ContactFilters {
  pageNumber: number
  campaignTitle?: string
  trackId?: string | number
  leadStatusId?: string | number
  tagIds?: string[]
  studentName?: string
  contactName?: string
  phoneNumber?: string
}
