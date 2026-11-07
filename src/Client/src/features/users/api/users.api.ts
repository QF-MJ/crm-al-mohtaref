import { api } from '@/api/client'
import type {
  UserDetail,
  UserFilters,
  UserLookups,
  UserUpdateInput,
  UsersPage,
} from '../types'

export const getUsers = (filters: UserFilters, signal?: AbortSignal) =>
  api.get<UsersPage>('/users', { params: filters, signal }).then((response) => response.data)

export const getUser = (id: number, signal?: AbortSignal) =>
  api.get<UserDetail>(`/users/${id}`, { signal }).then((response) => response.data)

export const updateUser = (id: number, body: UserUpdateInput) =>
  api.put<void>(`/users/${id}`, body).then((response) => response.data)

export const getUserLookups = (signal?: AbortSignal) =>
  api
    .get<UserLookups>('/lookups', {
      params: { include: ['Departments', 'Roles'] },
      paramsSerializer: { indexes: null },
      signal,
    })
    .then((response) => ({
      departments: response.data.departments ?? [],
      roles: response.data.roles ?? [],
    }))
