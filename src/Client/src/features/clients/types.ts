import type { LookupItem, TagLookup } from '@/features/lookups/types'

export interface ContactStudent {
  id: number
  name: string
}

export interface ContactListItem {
  id: number
  name: string
  students?: ContactStudent[]
}

export interface ContactsPage {
  items: ContactListItem[]
  page: {
    pageNumber: number
    hasNextPage: boolean
  }
}

export interface ContactFilters {
  pageNumber: number
  campaignTitle?: string
  trackId?: number
  leadStatusId?: number
  tagIds?: number[]
  studentName?: string
  contactName?: string
  phoneNumber?: string
}

export type ContactLookups = {
  tracks: LookupItem[]
  leadStatuses: LookupItem[]
  tags?: TagLookup[]
}
