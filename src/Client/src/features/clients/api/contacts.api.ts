import { api } from '@/api/client'
import type { ContactsPage, ContactFilters } from '../types'

export function getContacts(filters: ContactFilters, signal?: AbortSignal) {
  return api
    .get<ContactsPage>('/contacts', {
      params: {
        PageNumber: filters.pageNumber,
        CampaignTitle: filters.campaignTitle,
        TrackId: filters.trackId,
        LeadStatusId: filters.leadStatusId,
        TagIds: filters.tagIds,
        StudentName: filters.studentName,
        ContactName: filters.contactName,
        PhoneNumber: filters.phoneNumber,
      },
      paramsSerializer: { indexes: null },
      signal,
    })
    .then((response) => response.data)
}
