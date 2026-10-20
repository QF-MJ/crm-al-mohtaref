import { http, HttpResponse, delay } from 'msw'
import { env } from '@/lib/env'
import { ROLE_INHERITS, type Role } from '@/permissions/roles'

const roles = Object.keys(ROLE_INHERITS) as Role[]

export const authHandlers = [
  // للتجربة: اسم المستخدم = اسم الدور (مثل admin أو salesManager) وأي كلمة مرور غير فارغة
  http.post(`${env.apiUrl}/auth/login`, async ({ request }) => {
    const { username, password } = (await request.json()) as { username: string; password: string }
    await delay(300)
    if (!roles.includes(username as Role) || !password) {
      return HttpResponse.json({ message: 'بيانات الدخول غير صحيحة' }, { status: 401 })
    }
    return HttpResponse.json({
      token: 'mock-token',
      user: { id: '1', name: username, role: username },
    })
  }),
]
