/** معرّفات القوائم Guid كما يرسلها الباك */
export type LookupId = string

export interface LookupItem {
  id: LookupId
  name: string
}

// GET /api/lookups يعيد فقط المجموعات المطلوبة في include
export interface LookupsResponse {
  departments?: LookupItem[] | null
  roles?: LookupItem[] | null
  tracks?: LookupItem[] | null
  leadStatuses?: LookupItem[] | null
  tags?: LookupItem[] | null
}

export type LookupType = 'Departments' | 'Roles' | 'Tracks' | 'LeadStatuses' | 'Tags'
