export interface LookupItem {
  id: number
  name: string
}

export interface LookupsResponse {
  departments?: LookupItem[] | null
  roles?: LookupItem[] | null
  // UNVERIFIED: other lookup collections are retained only for dormant features.
  tracks?: LookupItem[]
  leadStatuses?: LookupItem[]
  tags?: Array<{ id: number; tag: string }>
  platforms?: LookupItem[]
  fields?: LookupItem[]
  campaignStatuses?: LookupItem[]
  questionTypes?: LookupItem[]
}

export type LookupType = 'Departments' | 'Roles'
