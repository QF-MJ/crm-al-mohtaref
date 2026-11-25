import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'
import type { LoginDto, PermissionDto } from '@/features/auth/types'

// ملاحظة: الأنواع فقط عبر type import، فلا اعتماد وقت تشغيل بين stores و features.

export interface SessionUser {
  /** Guid كما يرسله الباك */
  id: string
  fullName: string
  /** أزواج قسم + دور كما يرسلها الباك، دون أي تحويل */
  permissions: PermissionDto[]
  /** تاريخ انتهاء التوكن (ISO) — نتحقق منه قبل استخدام الجلسة */
  tokenExpiresAt: string | null
}

interface AuthState {
  user: SessionUser | null
  token: string | null
  refreshToken: string | null
  login: (dto: LoginDto) => void
  logout: () => void
}

export const AUTH_STORAGE_KEY = 'crm-session'

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      refreshToken: null,
      login: (dto) =>
        set({
          user: {
            id: dto.userId,
            fullName: dto.fullName ?? '',
            permissions: dto.permissions ?? [],
            tokenExpiresAt: dto.expiresIn ?? null,
          },
          token: dto.token,
          refreshToken: dto.refreshToken ?? null,
        }),
      logout: () => set({ user: null, token: null, refreshToken: null }),
    }),
    {
      name: AUTH_STORAGE_KEY,
      storage: createJSONStorage(() => localStorage),
      // تغيّر شكل الجلسة (إضافة refresh token)، فالجلسات السابقة تُهمل
      version: 2,
      migrate: () => ({ user: null, token: null, refreshToken: null }),
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        refreshToken: state.refreshToken,
      }),
    },
  ),
)

const EXPIRY_SKEW_MS = 30_000

export function isTokenExpired(user: SessionUser | null, now = Date.now()): boolean {
  if (!user?.tokenExpiresAt) return false
  const expiresAt = Date.parse(user.tokenExpiresAt)
  if (Number.isNaN(expiresAt)) return false
  return now >= expiresAt - EXPIRY_SKEW_MS
}

export function hasActiveSession(state: { user: SessionUser | null; token: string | null }) {
  return Boolean(state.user && state.token) && !isTokenExpired(state.user)
}
