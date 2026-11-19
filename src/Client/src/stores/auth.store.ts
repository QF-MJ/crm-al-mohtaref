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
  login: (dto: LoginDto) => void
  logout: () => void
}

export const AUTH_STORAGE_KEY = 'crm-session'

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      login: (dto) =>
        set({
          user: {
            id: dto.userId,
            fullName: dto.fullName ?? '',
            permissions: dto.permissions ?? [],
            tokenExpiresAt: dto.expiresIn ?? null,
          },
          token: dto.token,
        }),
      logout: () => set({ user: null, token: null }),
    }),
    {
      name: AUTH_STORAGE_KEY,
      storage: createJSONStorage(() => localStorage),
      // الإصدار 2: تغيّر شكل الجلسة (id نصي + تاريخ الانتهاء)، فجلسات الإصدار 1 تُهمل
      version: 2,
      migrate: () => ({ user: null, token: null }),
      partialize: (state) => ({ user: state.user, token: state.token }),
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
