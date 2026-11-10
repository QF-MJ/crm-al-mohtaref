import { api } from '@/api/client'
import type {
  UserDetail,
  UserFilters,
  UserUpdateInput,
  UsersPage,
} from '../types'

export const getUsers = (filters: UserFilters, signal?: AbortSignal) =>
  api
    .get<UsersPage>('/users', {
      params: {
        PageNumber: filters.pageNumber,
        Name: filters.name,
        AccountName: filters.accountName,
        Email: filters.email,
        IsActive: filters.isActive,
        RoleId: filters.roleId,
        DepartmentId: filters.departmentId,
      },
      signal,
    })
    .then((response) => response.data)

export const getUser = (id: number, signal?: AbortSignal) =>
  api.get<UserDetail>(`/users/${id}`, { signal }).then((response) => response.data)

export const updateUser = (id: number, body: UserUpdateInput) =>
  api.put<void>(`/users/${id}`, body).then((response) => response.data)
