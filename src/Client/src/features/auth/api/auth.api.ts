import { api } from '@/api/client'
import type { LoginDto, LoginRequest } from '../types'

export const LOGIN_PATH = '/auth/login'

export const loginRequest = (body: LoginRequest) =>
  api.post<LoginDto>(LOGIN_PATH, body).then((r) => r.data)
