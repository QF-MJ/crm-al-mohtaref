import type { PageInfo } from '@/types/common'

/** عنصر StudentDto: معرّف Guid واسم الطالب */
export interface ContactStudent {
  id: string
  name: string
}

/** عنصر ContactItemDto كما يعيده GET /api/contacts */
export interface Contact {
  id: string
  name: string
  students: ContactStudent[]
}

export interface ContactsPage {
  items: Contact[]
  page?: PageInfo
}

export interface ContactFilters {
  pageNumber: number
  campaignTitle?: string
  /** Guid */
  trackId?: string
  /** Guid */
  leadStatusId?: string
  /** Guids — الباك يطابق كل الوسوم المحددة (AND) */
  tagIds?: string[]
  studentName?: string
  contactName?: string
  phoneNumber?: string
}
