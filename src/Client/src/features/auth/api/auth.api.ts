import { api } from '@/api/client'
import type { LoginRequest, LoginResponse } from '../types'

export const loginRequest = (body: LoginRequest) =>
  api.post<LoginResponse>('/auth/login', body).then((r) => r.data)

export const logoutRequest = () => api.post<void>('/auth/logout').then(() => undefined)
