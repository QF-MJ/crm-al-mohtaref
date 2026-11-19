import { http, HttpResponse, delay } from 'msw'
import { env } from '@/lib/env'
import type { LoginDto, LoginRequest, RefreshRequest } from '@/features/auth/types'

// يحاكي سلوك الباك الفعلي (Seed تطوير):
//   كلمة المرور لكل الحسابات: 12345678
//   user01 مبيعات/موظف، user05 مبيعات/مدير، user10 تسويق/مدير،
//   user15 خدمة عملاء/مدير، user20 توجيه أكاديمي/مدير
//   admin: بلا صلاحيات (مثل الباك حالياً)
//   locked: يرجّع 403 (مثل الباك) | أي شيء آخر: 401 برسالة عامة
export const MOCK_PASSWORD = '12345678'

const dept = (departmentName: string, roleName: string) => [{ id: 0, departmentName, roleName }]

const accounts: Record<string, { userId: number; fullName: string; permissions: LoginDto['permissions'] }> = {
  admin: { userId: 1, fullName: 'مدير النظام', permissions: [] },
  user01: { userId: 2, fullName: 'أحمد خالد الحسن', permissions: dept('المبيعات', 'موظف') },
  user05: { userId: 6, fullName: 'مدير مبيعات تجريبي', permissions: dept('المبيعات', 'مدير') },
  user10: { userId: 11, fullName: 'مدير تسويق تجريبي', permissions: dept('التسويق', 'مدير') },
  user15: { userId: 16, fullName: 'مدير خدمة عملاء تجريبي', permissions: dept('خدمة العملاء', 'مدير') },
  user20: { userId: 21, fullName: 'مدير توجيه تجريبي', permissions: dept('التوجيه الأكاديمي', 'مدير') },
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
      expiresIn: 120,
      ...account,
    }
    return HttpResponse.json(body)
  }),

  // الباك الحقيقي لا يملك /auth/logout بعد، نبقيه بالـ mock للمستقبل
  http.post(`${env.apiUrl}/auth/logout`, async ({ request }) => {
    const { refreshToken } = (await request.json()) as RefreshRequest
    await delay(100)
    if (!refreshToken) {
      return HttpResponse.json({ status: 401, message: 'Invalid token' }, { status: 401 })
    }
    return new HttpResponse(null, { status: 204 })
  }),
]
