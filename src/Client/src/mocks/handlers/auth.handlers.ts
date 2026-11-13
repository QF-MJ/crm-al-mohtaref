import { http, HttpResponse, delay } from 'msw'
import { env } from '@/lib/env'
import type { LoginDto, LoginRequest } from '@/features/auth/types'

// يحاكي سلوك الباك الفعلي (Seed تطوير):
//   كلمة المرور لكل الحسابات: 12345678
//   user01 مبيعات/موظف، user05 مبيعات/مدير، user10 تسويق/مدير،
//   user15 خدمة عملاء/مدير، user20 توجيه أكاديمي/مدير
//   admin: مدير في قسم الإدارة
//   locked: يرجّع 403 (مثل الباك) | أي شيء آخر: 401 برسالة عامة
export const MOCK_PASSWORD = '12345678'

const dept = (
  departmentName: string,
  roleName: string,
): NonNullable<LoginDto['permissions']> => [
  { id: crypto.randomUUID(), departmentName, roleName },
]

const mockUserId = (id: number) => `00000000-0000-4000-8000-${String(id).padStart(12, '0')}`

const accounts: Record<
  string,
  { userId: string; fullName: string; permissions: LoginDto['permissions'] }
> = {
  admin: {
    userId: mockUserId(1),
    fullName: 'مدير النظام',
    permissions: [...dept('المبيعات', 'مدير'), ...dept('الإدارة', 'مدير')],
  },
  user01: {
    userId: mockUserId(2),
    fullName: 'أحمد خالد الحسن',
    permissions: dept('المبيعات', 'موظف'),
  },
  user05: {
    userId: mockUserId(6),
    fullName: 'مدير مبيعات تجريبي',
    permissions: dept('المبيعات', 'مدير'),
  },
  user10: {
    userId: mockUserId(11),
    fullName: 'مدير تسويق تجريبي',
    permissions: dept('التسويق', 'مدير'),
  },
  user15: {
    userId: mockUserId(16),
    fullName: 'مدير خدمة عملاء تجريبي',
    permissions: dept('خدمة العملاء', 'مدير'),
  },
  user20: {
    userId: mockUserId(21),
    fullName: 'مدير توجيه تجريبي',
    permissions: dept('التوجيه الأكاديمي', 'مدير'),
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
      token: 'mock-token',
      refreshToken: null,
      expiresIn: new Date(Date.now() + 2 * 60 * 60_000).toISOString(),
      ...account,
    }
    return HttpResponse.json(body)
  }),

]
