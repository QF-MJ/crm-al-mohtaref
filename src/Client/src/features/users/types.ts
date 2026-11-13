import type { PageInfo } from '@/types/common'
import type { LookupItem } from '@/features/lookups'

export type { PageInfo } from '@/types/common'
export type { LookupItem } from '@/features/lookups'

/** معرّف المستخدم Guid كما يرسله الباك */
export type UserId = string

export interface UserListItem {
  id: UserId
  accountName: string
  fullName: string
  email: string
  isActive: boolean
}

/** عنصر PermissionDto: اسم القسم والدور، والمعرّف Guid */
export interface UserPermission {
  id: string
  departmentName: string
  roleName: string
}

export interface UserDetail extends UserListItem {
  permissions: UserPermission[]
}

export interface UsersPage {
  items: UserListItem[]
  page?: PageInfo
}

export interface UserFilters {
  pageNumber: number
  name?: string
  accountName?: string
  email?: string
  isActive?: boolean
  /** Guid */
  roleId?: string
  /** Guid — إلزامي لمدير القسم، والباك يرفض الطلب بدونه */
  departmentId?: string
}

export interface UserLookups {
  departments: LookupItem[]
  roles: LookupItem[]
}

export interface UserPermissionInput {
  /** Guid */
  roleId: string
  /** Guid */
  departmentId: string
}

export interface UserUpdateInput {
  accountName: string
  fullName: string
  email: string
  isActive?: boolean | null
  permissions: UserPermissionInput[]
}

export interface UserCreateInput {
  accountName: string
  fullName: string
  email: string
  password: string
  permissions?: UserPermissionInput[]
}
