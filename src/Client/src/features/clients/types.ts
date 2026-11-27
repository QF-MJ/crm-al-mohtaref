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
  /** Guid */
  campaignId?: string
  /** Guid */
  trackId?: string
  /** enum LeadStatus */
  leadStatus?: number
  /** Guids — الباك يطابق كل الوسوم المحددة (AND) */
  tagIds?: string[]
  studentName?: string
  contactName?: string
  phoneNumber?: string
}

/** أقسام الملف الموحد. العنوان يترجم في الواجهة من contacts.profile.sections.<key> */
export type ProfileSectionKey =
  | 'opportunities'
  | 'communications'
  | 'subscriptions'
  | 'guidance'
  | 'support'

/** محمّل = توجد بيانات، فارغ = قسم متاح بلا سجلات، غير متاح = تعذّر جلب القسم */
export type ProfileSectionState = 'loaded' | 'empty' | 'unavailable'

export interface ProfileSectionRow {
  date?: string
  title: string
  description?: string
  meta?: string
}

export interface ContactProfileSection {
  key: ProfileSectionKey
  state: ProfileSectionState
  rows: ProfileSectionRow[]
}

export interface ContactProfileStudent {
  id: string
  name: string
  grade?: string
  sections: ContactProfileSection[]
}

/** GET /api/contacts/{id} — جهة التواصل هي نقطة التجميع، وكل طالب يحمل أقسامه منفصلة */
export interface ContactProfile {
  id: string
  name: string
  phoneNumber?: string
  email?: string
  city?: string
  updatedAt?: string
  students: ContactProfileStudent[]
}
