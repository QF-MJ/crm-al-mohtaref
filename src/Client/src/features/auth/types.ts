// أنواع الـ Auth مطابقة للرد الفعلي من الباك (تم التحقق منه من Swagger بتاريخ 11/11/2026).
// ملاحظة: الـ OpenAPI يكتب الحقول بحرف كبير (Token) لكن الباك يرجّعها camelCase (token).
// يجب توحيد الأمرين مع محمد، ونحن نتبع الرد الفعلي.

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
  token: string
  /** الباك يرجّعه null حالياً (لا يوجد refresh بعد) */
  refreshToken: string | null
  /** ExpiresIn is serialized as an ISO date-time by the backend. */
  expiresIn: string
  userId: string
  fullName?: string
  permissions?: PermissionDto[]
}

