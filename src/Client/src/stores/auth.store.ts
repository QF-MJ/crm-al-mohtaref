import { create } from 'zustand'
import type { Role } from '@/permissions/roles'

export interface SessionUser {
  id: string
  name: string
  role: Role
}

interface AuthState {
  user: SessionUser | null
  token: string | null
  login: (user: SessionUser, token: string) => void
  logout: () => void
}

// التوكن بالذاكرة فقط (أأمن من localStorage). لكن تحديث الصفحة يسجّل الخروج
// إلى أن يُحسم قرار التخزين مع الباك (httpOnly cookie أو refresh token)
export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  token: null,
  login: (user, token) => set({ user, token }),
  logout: () => set({ user: null, token: null }),
}))
