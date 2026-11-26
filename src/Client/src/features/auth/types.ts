// أنواع المصادقة مطابقة للرد الفعلي من الباك (LoginDto في Application/Features/Auth/Login).
// الباك يرسل camelCase، و`expiresIn` تاريخ انتهاء (DateTime) وليس عدد ثوانٍ،
// و`userId` و`id` معرّفات Guid وليست أرقاماً.

export interface PermissionDto {
  id: string
  departmentName: string
  roleName: string
}

export interface LoginRequest {
  accountName: string
  password: string
}

export interface LoginDto {
  /** Access Token (JWT) يُرسل مع كل طلب */
  token: string
  /** بصيغة "{guid}.{random}"، عمره 7 أيام، ويُبدَّل (rotation) مع كل تجديد */
  refreshToken: string | null
  /** تاريخ انتهاء صلاحية الـ Access Token بصيغة ISO (UTC) */
  expiresIn: string | null
  userId: string
  fullName: string
  permissions: PermissionDto[]
}

export interface RefreshTokenRequest {
  refreshToken: string
}

/** رد POST /auth/refresh: نفس شكل رد الدخول (RefreshTokenDto في الباك) */
export type RefreshTokenDto = LoginDto
