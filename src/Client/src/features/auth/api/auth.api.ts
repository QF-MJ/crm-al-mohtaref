import { api } from '@/api/client'
import { AUTH_PATHS } from '@/api/session'
import type { LoginDto, LoginRequest } from '../types'

// طلب التجديد نفسه في src/api/session.ts لأنه جزء من آلية الـ HTTP client

export const loginRequest = (body: LoginRequest) =>
  api.post<LoginDto>(AUTH_PATHS.login, body).then((r) => r.data)

// الباك يلغي الـ refresh token بالداتا بيز ويرجّع 204
export const logoutRequest = (refreshToken: string) =>
  api.post<void>(AUTH_PATHS.logout, { refreshToken }).then(() => undefined)
