import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'
import type { LoginDto, PermissionDto } from '@/features/auth/types'

// ملاحظة: القاعدة 3 بالـ conventions تمنع components/hooks/lib من استيراد features،
// لكن stores غير مذكورة. الأنواع فقط (type import) فلا يوجد اعتماد وقت التشغيل.

export interface SessionUser {
  id: string
  fullName: string
  /** أزواج قسم + دور كما يرسلها الباك، بدون أي تحويل */
  permissions: PermissionDto[]
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
          },
          token: dto.token,
          refreshToken: dto.refreshToken,
        }),
      logout: () => set({ user: null, token: null, refreshToken: null }),
    }),
    {
      name: AUTH_STORAGE_KEY,
      // localStorage قرار مؤقت لتسليم 28/11 (القرار الموثّق: الخيار 2)
      storage: createJSONStorage(() => localStorage),
      partialize: (s) => ({
        user: s.user,
        token: s.token,
        refreshToken: s.refreshToken,
      }),
    },
  ),
)
