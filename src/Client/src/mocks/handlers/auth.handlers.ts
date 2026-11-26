import { http, HttpResponse, delay } from 'msw'
import { env } from '@/lib/env'
import type {
  LoginDto,
  LoginRequest,
  PermissionDto,
  RefreshTokenRequest,
} from '@/features/auth/types'

// محاكاة مطابقة لرد الباك الحقيقي: معرّفات Guid، وexpiresIn تاريخ ISO، وrefresh/logout مع تبديل التوكن.
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

// ── التوكنات ─────────────────────────────────────────────────────────────
// Access Token وهمي يحمل الحساب وموعد الانتهاء: "mock-token.{account}.{expiresAtMs}".
// عمره قصير عن قصد (دقيقتان) حتى تُجرَّب آلية التجديد أثناء التطوير بالمحاكاة.
const ACCESS_TOKEN_TTL_MS = 2 * 60_000
const ACCESS_TOKEN_PREFIX = 'mock-token'

// Refresh Token بنفس صيغة الباك "{guid}.{random}". نحفظ سجلها في localStorage
// حتى تبقى صالحة بعد إعادة تحميل الصفحة (المحاكاة تعيش داخل الصفحة نفسها).
const REFRESH_STORE_KEY = 'crm-mock-refresh-tokens'

interface MockRefreshRecord {
  accountName: string
  revoked: boolean
}

type RefreshStore = Record<string, MockRefreshRecord>

function readRefreshStore(): RefreshStore {
  try {
    return JSON.parse(localStorage.getItem(REFRESH_STORE_KEY) ?? '{}') as RefreshStore
  } catch {
    return {}
  }
}

const writeRefreshStore = (store: RefreshStore) =>
  localStorage.setItem(REFRESH_STORE_KEY, JSON.stringify(store))

function issueSession(accountName: string): LoginDto {
  const account = accounts[accountName]
  const expiresAt = Date.now() + ACCESS_TOKEN_TTL_MS
  const refreshId = crypto.randomUUID()

  const store = readRefreshStore()
  store[refreshId] = { accountName, revoked: false }
  writeRefreshStore(store)

  return {
    token: `${ACCESS_TOKEN_PREFIX}.${accountName}.${expiresAt}`,
    refreshToken: `${refreshId}.${crypto.randomUUID()}`,
    expiresIn: new Date(expiresAt).toISOString(),
    ...account,
  }
}

/** الحساب صاحب الـ Access Token إذا كان صالحاً وغير منتهٍ، وإلا null */
function accountFromAuthHeader(header: string | null): string | null {
  const [prefix, accountName, expiresAt] = header?.replace(/^Bearer /, '').split('.') ?? []
  if (prefix !== ACCESS_TOKEN_PREFIX || !accounts[accountName]) return null
  return Number(expiresAt) > Date.now() ? accountName : null
}

const REFRESH_TOKEN_PATTERN =
  /^([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})\.(.+)$/i

const errorResponse = (status: number, message: string) =>
  HttpResponse.json({ status, message }, { status })

const AUTH_PATHS = ['/auth/login', '/auth/refresh', '/auth/logout']

/**
 * يوضع أول القائمة: أي طلب API (عدا المصادقة) بلا Access Token صالح يرجع 401، كما يفعل [Authorize].
 * إذا كان التوكن صالحاً لا نرجّع شيئاً، فيكمل MSW إلى الـ handler المطابق.
 */
export const mockAuthGate = http.all(`${env.apiUrl}/*`, ({ request }) => {
  const { pathname } = new URL(request.url)
  if (AUTH_PATHS.some((path) => pathname.endsWith(path))) return undefined
  if (accountFromAuthHeader(request.headers.get('Authorization'))) return undefined
  return errorResponse(401, 'Unauthorized')
})

export const authHandlers = [
  http.post(`${env.apiUrl}/auth/login`, async ({ request }) => {
    const { accountName, password } = (await request.json()) as LoginRequest
    await delay(300)

    if (accountName === 'locked') {
      return errorResponse(403, 'الحساب مقفل حالياً. يرجى المحاولة مجدداً لاحقاً')
    }

    if (!accounts[accountName] || password !== MOCK_PASSWORD) {
      return errorResponse(401, 'كلمة السر أو اسم الحساب غير صحيحين')
    }

    return HttpResponse.json(issueSession(accountName))
  }),

  // مثل الباك: يلغي التوكن القديم ويصدر زوجاً جديداً (rotation).
  // استعمال توكن ملغى مرة ثانية = سرقة محتملة، فتُلغى كل جلسات الحساب.
  http.post(`${env.apiUrl}/auth/refresh`, async ({ request }) => {
    const { refreshToken } = (await request.json()) as RefreshTokenRequest
    await delay(200)

    const match = REFRESH_TOKEN_PATTERN.exec(refreshToken ?? '')
    if (!match) return errorResponse(400, 'رمز التحديث غير صالح')

    const store = readRefreshStore()
    const record = store[match[1]]
    if (!record) return errorResponse(401, 'Refresh token not found')

    if (record.revoked) {
      for (const item of Object.values(store)) {
        if (item.accountName === record.accountName) item.revoked = true
      }
      writeRefreshStore(store)
      return errorResponse(401, 'Refresh Token expired')
    }

    record.revoked = true
    writeRefreshStore(store)
    return HttpResponse.json(issueSession(record.accountName))
  }),

  http.post(`${env.apiUrl}/auth/logout`, async ({ request }) => {
    const { refreshToken } = (await request.json()) as RefreshTokenRequest
    await delay(150)

    const match = REFRESH_TOKEN_PATTERN.exec(refreshToken ?? '')
    if (!match) return errorResponse(400, 'رمز التحديث غير صالح')

    const store = readRefreshStore()
    const record = store[match[1]]
    if (!record) return errorResponse(404, 'Refresh token not found')

    record.revoked = true
    writeRefreshStore(store)
    return new HttpResponse(null, { status: 204 })
  }),
]
