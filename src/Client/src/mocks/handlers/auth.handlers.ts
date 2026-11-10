import { http, HttpResponse, delay } from 'msw'
import { env } from '@/lib/env'
import { ROLE_INHERITS, type Role } from '@/permissions/roles'
import type { LoginRequest } from '@/features/auth/types'

const roles = Object.keys(ROLE_INHERITS) as Role[]

const rolePermissions: Record<Role, { departmentName: string; roleName: string }> = {
  employee: { departmentName: 'المبيعات', roleName: 'موظف' },
  salesEmployee: { departmentName: 'المبيعات', roleName: 'موظف' },
  salesManager: { departmentName: 'المبيعات', roleName: 'مدير' },
  marketingEmployee: { departmentName: 'التسويق', roleName: 'موظف' },
  marketingManager: { departmentName: 'التسويق', roleName: 'مدير' },
  academicAdvisor: { departmentName: 'التوجيه الأكاديمي', roleName: 'موظف' },
  customerService: { departmentName: 'خدمة العملاء', roleName: 'موظف' },
  admin: { departmentName: 'الإدارة', roleName: 'مدير' },
}

export const authHandlers = [
  http.post(`${env.apiUrl}/auth/login`, async ({ request }) => {
    const { accountName, password } = (await request.json()) as LoginRequest
    await delay(300)
    if (!roles.includes(accountName as Role) || !password) {
      return HttpResponse.json({ status: 401, message: 'بيانات الدخول غير صحيحة' }, { status: 401 })
    }
    return HttpResponse.json({
      token: 'mock-token',
      refreshToken: 'mock-refresh-token',
      expiresIn: 3600,
      userId: 1,
      fullName: accountName,
      permissions: [{ id: 1, ...rolePermissions[accountName as Role] }],
    })
  }),
]
