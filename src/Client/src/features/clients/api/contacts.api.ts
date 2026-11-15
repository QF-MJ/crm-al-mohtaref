import { api } from '@/api/client'
import type { Contact, ContactsPage, ContactFilters } from '../types'

// الاستجابة الفعلية: { items: [{ id, name, students: [{ id, name }] }], page: { pageNumber, hasNextPage } }
// pageNumber القادم من /contacts غير موثوق (الباك يرسل PageSize بدلاً منه)،
// لذلك الواجهة تتتبع رقم الصفحة محلياً وتعتمد hasNextPage فقط.
export function getContacts(filters: ContactFilters, signal?: AbortSignal) {
  return api
    .get<ContactsPage>('/contacts', {
      params: {
        pageNumber: filters.pageNumber,
        campaignTitle: filters.campaignTitle,
        trackId: filters.trackId,
        leadStatusId: filters.leadStatusId,
        tagIds: filters.tagIds,
        studentName: filters.studentName,
        contactName: filters.contactName,
        phoneNumber: filters.phoneNumber,
      },
      // مفاتيح متكررة: tagIds=a&tagIds=b
      paramsSerializer: { indexes: null },
      signal,
    })
    .then((response) => normalizePage(response.data))
}

function normalizePage(data: ContactsPage): ContactsPage {
  return {
    items: Array.isArray(data?.items) ? data.items.map(normalizeContact) : [],
    page: data?.page,
  }
}

function normalizeContact(contact: Contact): Contact {
  return {
    id: String(contact?.id ?? ''),
    name: contact?.name ?? '',
    students: Array.isArray(contact?.students)
      ? contact.students.map((student) => ({
          id: String(student?.id ?? ''),
          name: student?.name ?? '',
        }))
      : [],
  }
}
