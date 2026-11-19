import { api } from '@/api/client'
import type { LoginDto, LoginRequest, RefreshRequest } from '../types'

export const LOGIN_PATH = '/auth/login'

export const loginRequest = (body: LoginRequest) =>
  api.post<LoginDto>(LOGIN_PATH, body).then((r) => r.data)

// الرد 204 بدون body
export const logoutRequest = (body: RefreshRequest) =>
  api.post<void>('/auth/logout', body).then(() => undefined)
