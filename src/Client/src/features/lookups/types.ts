/** معرّفات القوائم Guid كما يرسلها الباك */
export type LookupId = string

export interface LookupItem {
  id: LookupId
  name: string
}

// GET /api/lookups يعيد فقط المجموعات المطلوبة في include
// ملاحظة: الأدوار وحالات العميل أصبحت enums ثابتة في الفرونت ولا تُجلب من الـ API.
export interface LookupsResponse {
  departments?: LookupItem[] | null
  tracks?: LookupItem[] | null
  tags?: LookupItem[] | null
}

export type LookupType = 'Departments' | 'Tracks' | 'Tags'
