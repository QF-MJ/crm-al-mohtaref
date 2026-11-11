import { api } from '@/api/client'
import type {
  UserCreateInput,
  UserDetail,
  UserFilters,
  UserUpdateInput,
  UsersPage,
} from '../types'

export const getUsers = (filters: UserFilters, signal?: AbortSignal) =>
  api
    .get<UsersPage>('/users', {
      params: {
        pageNumber: filters.pageNumber,
        name: filters.name,
        accountName: filters.accountName,
        email: filters.email,
        isActive: filters.isActive,
        roleId: filters.roleId,
        departmentId: filters.departmentId,
      },
      signal,
    })
    .then((response) => response.data)

export const getUser = (id: number, signal?: AbortSignal) =>
  api.get<UserDetail>(`/users/${id}`, { signal }).then((response) => response.data)

export const addUser = (body: UserCreateInput) =>
  api.post<number>('/users', body).then((response) => response.data)

export const updateUser = (id: number, body: UserUpdateInput) =>
  api.put<void>(`/users/${id}`, body).then((response) => response.data)
