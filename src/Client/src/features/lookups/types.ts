export interface LookupItem {
  id: number
  name: string
}

export interface TagLookup {
  id: number
  tag: string
}

export interface LookupsResponse {
  tracks?: LookupItem[]
  leadStatuses?: LookupItem[]
  tags?: TagLookup[]
  platforms?: LookupItem[]
  fields?: LookupItem[]
  campaignStatuses?: LookupItem[]
  questionTypes?: LookupItem[]
  departments?: LookupItem[]
  roles?: LookupItem[]
}
