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
  token: string
  /** الباك يرجّعه null حالياً: لا يوجد refresh endpoint */
  refreshToken: string | null
  /** تاريخ انتهاء صلاحية التوكن بصيغة ISO */
  expiresIn: string | null
  userId: string
  fullName: string
  permissions: PermissionDto[]
}
