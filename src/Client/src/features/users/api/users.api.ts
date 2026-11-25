import { api } from '@/api/client'
import type {
  UserCreateInput,
  UserDetail,
  UserFilters,
  UserId,
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

export const getUser = (id: UserId, signal?: AbortSignal) =>
  api
    .get<UserDetail>(`/users/${encodeURIComponent(id)}`, { signal })
    .then((response) => response.data)

// الباك يرجّع Guid الجديد كنص في جسم الرد 201
export const addUser = (body: UserCreateInput) =>
  api.post<UserId>('/users', body).then((response) => response.data)

export const updateUser = (id: UserId, body: UserUpdateInput) =>
  api.put<void>(`/users/${encodeURIComponent(id)}`, body).then(() => undefined)
