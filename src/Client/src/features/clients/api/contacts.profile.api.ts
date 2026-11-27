import { api } from '@/api/client'
import type {
  ContactProfile,
  ContactProfileSection,
  ContactProfileStudent,
  ProfileSectionKey,
  ProfileSectionRow,
  ProfileSectionState,
} from '../types'

const SECTION_KEYS: ProfileSectionKey[] = [
  'opportunities',
  'communications',
  'subscriptions',
  'guidance',
  'support',
]

const SECTION_STATES: ProfileSectionState[] = ['loaded', 'empty', 'unavailable']

/** GET /api/contacts/{id} — الملف الموحد لجهة التواصل والطلاب المرتبطين بها */
export function getContactProfile(id: string, signal?: AbortSignal) {
  return api
    .get<ContactProfile>(`/contacts/${encodeURIComponent(id)}`, { signal })
    .then((response) => normalizeProfile(response.data))
}

// الباك قد يرسل أسماء خصائص بأحرف كبيرة أو أقساماً ناقصة؛ normalize تضمن شكلاً واحداً للواجهة
function normalizeProfile(data: ContactProfile | undefined): ContactProfile {
  return {
    id: String(data?.id ?? ''),
    name: data?.name ?? '',
    phoneNumber: optionalText(data?.phoneNumber),
    email: optionalText(data?.email),
    city: optionalText(data?.city),
    updatedAt: optionalText(data?.updatedAt),
    students: Array.isArray(data?.students) ? data.students.map(normalizeStudent) : [],
  }
}

function normalizeStudent(student: ContactProfileStudent): ContactProfileStudent {
  return {
    id: String(student?.id ?? ''),
    name: student?.name ?? '',
    grade: optionalText(student?.grade),
    sections: Array.isArray(student?.sections) ? student.sections.map(normalizeSection) : [],
  }
}

function normalizeSection(section: ContactProfileSection): ContactProfileSection {
  const key = SECTION_KEYS.includes(section?.key) ? section.key : 'opportunities'
  const state = SECTION_STATES.includes(section?.state) ? section.state : 'unavailable'
  return {
    key,
    state,
    rows: Array.isArray(section?.rows) ? section.rows.map(normalizeRow) : [],
  }
}

function normalizeRow(row: ProfileSectionRow): ProfileSectionRow {
  return {
    date: optionalText(row?.date),
    title: row?.title ?? '',
    description: optionalText(row?.description),
    meta: optionalText(row?.meta),
  }
}

function optionalText(value: unknown): string | undefined {
  const text = typeof value === 'string' || typeof value === 'number' ? String(value).trim() : ''
  return text || undefined
}
