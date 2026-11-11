import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'
import type { Role } from '@/permissions/roles'
import type { LoginResponse } from '@/features/auth/types'

export interface SessionUser {
  id: string
  accountName: string
  name: string
  roles: Role[]
}

interface AuthState {
  user: SessionUser | null
  token: string | null
  login: (response: LoginResponse, accountName: string) => void
  logout: () => void
}

export const AUTH_STORAGE_KEY = 'crm-session'

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      login: (response, accountName) =>
        set({
          user: {
            id: String(response.userId),
            accountName,
            name: response.fullName ?? '',
            roles: mapBackendPermissions(response.permissions ?? [], accountName),
          },
          token: response.token,
        }),
      logout: () => set({ user: null, token: null }),
    }),
    {
      name: AUTH_STORAGE_KEY,
      version: 3,
      migrate: () => ({ user: null, token: null }),
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({ user: state.user, token: state.token }),
    },
  ),
)

function mapBackendPermissions(
  permissions: NonNullable<LoginResponse['permissions']>,
  accountName: string,
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

  if (accountName.trim().toLocaleLowerCase() === 'admin') mapped.add('admin')
  return [...mapped]
}
