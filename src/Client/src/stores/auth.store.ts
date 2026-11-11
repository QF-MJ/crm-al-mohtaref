import { create } from 'zustand'
import type { Role } from '@/permissions/roles'
import type { LoginResponse } from '@/features/auth/types'

export interface SessionUser {
<<<<<<< Updated upstream
  id: string
  name: string
  roles: Role[]
=======
  id: number
  accountName: string
  fullName: string
  /** أزواج قسم + دور كما يرسلها الباك، بدون أي تحويل */
  permissions: PermissionDto[]
>>>>>>> Stashed changes
}

interface AuthState {
  user: SessionUser | null
  token: string | null
<<<<<<< Updated upstream
  login: (response: LoginResponse) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  token: null,
  login: (response) =>
    set({
      user: {
        id: String(response.userId),
        name: response.fullName ?? '',
        roles: mapBackendPermissions(response.permissions ?? []),
      },
      token: response.token,
    }),
  logout: () => set({ user: null, token: null }),
}))

function mapBackendPermissions(
  permissions: NonNullable<LoginResponse['permissions']>,
): Role[] {
  const mapped = new Set<Role>()
  for (const permission of permissions) {
    const roleName = permission.roleName.trim().toLocaleLowerCase()
    const departmentName = permission.departmentName.trim().toLocaleLowerCase()
    const isManager = roleName === 'مدير' || roleName === 'manager'
    const isEmployee = roleName === 'موظف' || roleName === 'employee'

    if (departmentName === 'الإدارة' || departmentName === 'administration') {
      if (isManager) mapped.add('admin')
    } else if (departmentName === 'المبيعات' || departmentName === 'sales') {
      if (isManager) mapped.add('salesManager')
      else if (isEmployee) mapped.add('salesEmployee')
    } else if (departmentName === 'التسويق' || departmentName === 'marketing') {
      if (isManager) mapped.add('marketingManager')
      else if (isEmployee) mapped.add('marketingEmployee')
    } else if (departmentName === 'خدمة العملاء' || departmentName === 'customer service') {
      if (isManager || isEmployee) mapped.add('customerService')
    } else if (
      departmentName === 'التوجيه الأكاديمي' ||
      departmentName === 'academic guidance'
    ) {
      if (isManager || isEmployee) mapped.add('academicAdvisor')
    }
  }
  return [...mapped]
}
=======
  refreshToken: string | null
  login: (dto: LoginDto, accountName: string) => void
  logout: () => void
}

export const AUTH_STORAGE_KEY = 'crm-session'

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      refreshToken: null,
      login: (dto, accountName) =>
        set({
          user: {
            id: dto.userId,
            accountName,
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
      version: 1,
      migrate: () => ({ user: null, token: null, refreshToken: null }),
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
>>>>>>> Stashed changes
