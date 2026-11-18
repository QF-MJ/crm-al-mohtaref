import type { LookupItem } from '@/features/lookups'

export type RoleValue = 1 | 2

export interface RoleOption {
  value: RoleValue
  label: string
}

// الأدوار ثابتة (لم تعد تُجلب من الـ API). القيم تطابق enum Role في الباك.
export const ROLES: readonly RoleOption[] = [
  { value: 1, label: 'موظف' },
  { value: 2, label: 'مدير' },
] as const

// شكل متوافق مع LookupItem للاستخدام في القوائم المنسدلة (id نصي = قيمة enum)
export const ROLE_ITEMS: LookupItem[] = ROLES.map((role) => ({
  id: String(role.value),
  name: role.label,
}))

export function roleValueFromName(name?: string | null): RoleValue | undefined {
  const normalized = name?.trim()
  if (!normalized) return undefined
  const match = ROLES.find((role) => role.label === normalized)
  return match?.value
}

export function roleLabel(value: number | string | undefined): string | undefined {
  if (value === undefined) return undefined
  const numeric = typeof value === 'string' ? Number(value) : value
  return ROLES.find((role) => role.value === numeric)?.label
}
