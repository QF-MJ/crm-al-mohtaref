export type LookupId = string | number

export interface LookupItem {
  id: LookupId
  name: string
}

export interface NumericLookupItem {
  id: number
  name: string
}

export interface LookupsResponse {
  departments?: LookupItem[] | null
  roles?: LookupItem[] | null
  tracks?: LookupItem[]
  leadStatuses?: LookupItem[]
  tags?: Array<{ id: number; tag: string }>
  // These collections are not returned by the current backend lookup endpoint.
  platforms?: NumericLookupItem[]
  fields?: NumericLookupItem[]
  campaignStatuses?: NumericLookupItem[]
  questionTypes?: NumericLookupItem[]
}

export type LookupType = 'Departments' | 'Roles' | 'Tracks' | 'LeadStatuses'
