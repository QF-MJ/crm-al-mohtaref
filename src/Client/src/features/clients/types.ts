export interface ContactStudent {
  id: string
  name: string
}

export interface Contact {
  id: string
  name: string
  students: ContactStudent[]
}

export interface ContactsPage {
  items: Contact[]
  page: {
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
