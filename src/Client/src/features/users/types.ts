export interface UserListItem {
  id: number
  accountName: string
  fullName: string
  email: string
  isActive: boolean
}

export interface UserPermission {
  id: number
  departmentName: string
  roleName: string
}

export interface UserDetail extends UserListItem {
  permissions: UserPermission[]
}

export interface UserPage {
  pageNumber: number
  hasNextPage: boolean
}

export interface UsersPage {
  items: UserListItem[]
  page: UserPage
}

export interface UserFilters {
  pageNumber: number
  name?: string
  accountName?: string
  email?: string
  isActive?: boolean
  roleId?: number
  departmentId?: number
}

export interface LookupItem {
  id: number
  name: string
}

export interface UserLookups {
  departments: LookupItem[]
  roles: LookupItem[]
}

export interface UserPermissionInput {
  roleId: number
  departmentId: number
}

export interface UserUpdateInput {
  accountName: string
  fullName: string
  email: string
  isActive: boolean
  permissions: UserPermissionInput[]
}

export type UserCreateInput = Pick<UserUpdateInput, 'accountName' | 'fullName' | 'email' | 'permissions'>
