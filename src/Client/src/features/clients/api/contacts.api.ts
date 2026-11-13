import { api } from '@/api/client'
import type { Contact, ContactFilters, ContactStudent, ContactsPage } from '../types'

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
        pageNumber: filters.pageNumber,
        campaignTitle: filters.campaignTitle,
        trackId: filters.trackId,
        leadStatusId: filters.leadStatusId,
        tagIds: filters.tagIds,
        studentName: filters.studentName,
        contactName: filters.contactName,
        phoneNumber: filters.phoneNumber,
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
  if (!Array.isArray(rawItems)) {
    throw new InvalidContactsResponseError('Expected an items array.')
  }
  if (!isRecord(rawPage)) {
    throw new InvalidContactsResponseError('Expected page information.')
  }

  return {
    items: rawItems.map(parseContact),
    page: parsePageInfo(rawPage),
  }
}

function parsePageInfo(value: Record<string, unknown>): ContactsPage['page'] {
  const pageNumber = value.pageNumber ?? value.PageNumber
  const hasNextPage = value.hasNextPage ?? value.HasNextPage
  if (
    typeof pageNumber !== 'number' ||
    !Number.isInteger(pageNumber) ||
    pageNumber < 1 ||
    typeof hasNextPage !== 'boolean'
  ) {
    throw new InvalidContactsResponseError('Invalid page information.')
  }
  return { pageNumber, hasNextPage }
}

function parseContact(value: unknown): Contact {
  if (!isRecord(value)) {
    throw new InvalidContactsResponseError('Contact item must be an object.')
  }

  const id = value.id ?? value.Id
  const name = value.name ?? value.Name
  const students = value.students ?? value.Students
  if (typeof id !== 'string' || typeof name !== 'string' || !Array.isArray(students)) {
    throw new InvalidContactsResponseError('Contact item is missing required fields.')
  }
  return { id, name, students: students.map(parseStudent) }
}

function parseStudent(value: unknown): ContactStudent {
  if (!isRecord(value)) {
    throw new InvalidContactsResponseError('Student item must be an object.')
  }
  const id = value.id ?? value.Id
  const name = value.name ?? value.Name
  if (typeof id !== 'string' || typeof name !== 'string') {
    throw new InvalidContactsResponseError('Student item is missing required fields.')
  }
  return { id, name }
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}
