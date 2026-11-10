export interface LoginRequest {
  accountName: string
  password: string
}

export interface AuthPermission {
  id: number
  departmentName: string
  roleName: string
}

export interface LoginResponse {
  token: string
  refreshToken: string | null
  expiresIn: number
  userId: number
  fullName: string
  permissions: AuthPermission[]
}
