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
  // غير مدعومة في الباك بعد (LookupType فيه Departments و Tracks فقط): متوفرة في الـ mock فقط
  campaignStatuses?: LookupItem[] | null
  platforms?: LookupItem[] | null
}

// CampaignStatuses و Platforms غير مدعومة في الباك بعد؛ useCampaignLookups يرجع للمسارات عند رفضها
export type LookupType =
  | 'Departments'
  | 'Roles'
  | 'Tracks'
  | 'LeadStatuses'
  | 'Tags'
  | 'Campaigns'
  | 'CampaignStatuses'
  | 'Platforms'
