import { http, HttpResponse, delay } from 'msw'
import { env } from '@/lib/env'
import type { LoginDto, LoginRequest, PermissionDto } from '@/features/auth/types'

// محاكاة مطابقة لرد الباك الحقيقي: معرّفات Guid، وexpiresIn تاريخ ISO، ولا logout endpoint.
// كلمة المرور لكل الحسابات التجريبية: 12345678
//   admin      → مدير / الإدارة              (صلاحية كاملة)
//   sales      → مدير / المبيعات             (عملاء + موظفو قسمه)
//   salesemp   → موظف / المبيعات             (حسابه فقط)
//   marketing  → مدير / التسويق
//   support    → مدير / خدمة العملاء
//   academic   → مدير / التوجيه الأكاديمي
export const MOCK_PASSWORD = '12345678'

const guid = (suffix: number) => `00000000-0000-0000-0000-${String(suffix).padStart(12, '0')}`

const pairs = (departmentName: string, roleName: string, suffix: number): PermissionDto[] => [
  { id: guid(suffix), departmentName, roleName },
]

const accounts: Record<string, { userId: string; fullName: string; permissions: PermissionDto[] }> =
  {
    admin: {
      userId: guid(1),
      fullName: 'مدير النظام',
      permissions: pairs('الإدارة', 'مدير', 9001),
    },
    sales: {
      userId: guid(2),
      fullName: 'مدير المبيعات',
      permissions: pairs('المبيعات', 'مدير', 9002),
    },
    salesemp: {
      userId: guid(3),
      fullName: 'موظف مبيعات',
      permissions: pairs('المبيعات', 'موظف', 9003),
    },
    marketing: {
      userId: guid(4),
      fullName: 'مدير التسويق',
      permissions: pairs('التسويق', 'مدير', 9004),
    },
    support: {
      userId: guid(5),
      fullName: 'مدير خدمة العملاء',
      permissions: pairs('خدمة العملاء', 'مدير', 9005),
    },
    academic: {
      userId: guid(6),
      fullName: 'مدير التوجيه الأكاديمي',
      permissions: pairs('التوجيه الأكاديمي', 'مدير', 9006),
    },
  }

export const authHandlers = [
  http.post(`${env.apiUrl}/auth/login`, async ({ request }) => {
    const { accountName, password } = (await request.json()) as LoginRequest
    await delay(300)

    if (accountName === 'locked') {
      return HttpResponse.json(
        { status: 403, message: 'الحساب مقفل حالياً. يرجى المحاولة مجدداً لاحقاً' },
        { status: 403 },
      )
    }

    const account = accounts[accountName]
    if (!account || password !== MOCK_PASSWORD) {
      return HttpResponse.json(
        { status: 401, message: 'كلمة السر أو اسم الحساب غير صحيحين' },
        { status: 401 },
      )
    }

    const body: LoginDto = {
      token: `mock-token.${accountName}`,
      refreshToken: null,
      expiresIn: new Date(Date.now() + 120 * 60_000).toISOString(),
      ...account,
    }
    return HttpResponse.json(body)
  }),
]
