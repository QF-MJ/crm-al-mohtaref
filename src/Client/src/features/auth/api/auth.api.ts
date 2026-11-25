import { api } from '@/api/client'
import type { LoginDto, LoginRequest } from '../types'

export const LOGIN_PATH = '/auth/login'

export const loginRequest = (body: LoginRequest) =>
  api.post<LoginDto>('/auth/login', body).then((r) => r.data)

export const logoutRequest = (refreshToken: string) =>
  api.post<void>('/auth/logout', { refreshToken }).then(() => undefined)
// لا يوجد logout/refresh في الباك حالياً، لذلك الخروج محلي فقط (auth.store).
