import type { SessionUser } from '@/stores/auth.store'

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  user: SessionUser
}
