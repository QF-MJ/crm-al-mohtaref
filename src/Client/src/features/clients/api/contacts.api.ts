import { api } from '@/api/client'
import type { ContactFilters, ContactListItem, ContactsPage, ContactStudent } from '../types'

export class InvalidContactsResponseError extends Error {
  constructor(message: string) {
    super(message)
    this.name = 'InvalidContactsResponseError'
  }
}

export function getContacts(filters: ContactFilters, signal?: AbortSignal) {
  return api
    .get<unknown>('/contacts', {
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
    .then((response) => parseContactsPage(response.data))
}

function parseContactsPage(value: unknown): ContactsPage {
  if (!isRecord(value)) {
    throw new InvalidContactsResponseError('Expected an object.')
  }

  const rawItems = value.items ?? value.Items
  const rawPage = value.page ?? value.Page
  if (!Array.isArray(rawItems) || !isRecord(rawPage)) {
    throw new InvalidContactsResponseError('Expected items and page.')
  }

  const pageNumber = rawPage.pageNumber ?? rawPage.PageNumber
  const hasNextPage = rawPage.hasNextPage ?? rawPage.HasNextPage
  if (typeof pageNumber !== 'number' || typeof hasNextPage !== 'boolean') {
    throw new InvalidContactsResponseError('Invalid page information.')
  }

  return {
    items: rawItems.map(parseContact),
    page: { pageNumber, hasNextPage },
  }
}

function parseContact(value: unknown): ContactListItem {
  if (!isRecord(value)) {
    throw new InvalidContactsResponseError('Contact item must be an object.')
  }

  const id = value.id ?? value.Id
  const name = value.name ?? value.Name
  const rawStudents = value.students ?? value.Students
  if (typeof id !== 'number' || typeof name !== 'string') {
    throw new InvalidContactsResponseError('Contact item is missing its id or name.')
  }
  if (rawStudents !== undefined && !Array.isArray(rawStudents)) {
    throw new InvalidContactsResponseError('Students must be an array.')
  }

  return {
    id,
    name,
    students: rawStudents?.map(parseStudent),
  }
}

function parseStudent(value: unknown): ContactStudent {
  if (!isRecord(value)) {
    throw new InvalidContactsResponseError('Student item must be an object.')
  }

  const id = value.id ?? value.Id
  const name = value.name ?? value.Name
  if (typeof id !== 'number' || typeof name !== 'string') {
    throw new InvalidContactsResponseError('Student item is missing its id or name.')
  }

  return { id, name }
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}
