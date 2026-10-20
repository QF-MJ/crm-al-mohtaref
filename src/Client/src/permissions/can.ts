import { ROLE_INHERITS, type Role } from './roles'
import { ROLE_PERMISSIONS, type Permission } from './permissions'

// الدور + كل الأدوار التي يرثها (بشكل متسلسل)
export function getEffectiveRoles(role: Role, seen = new Set<Role>()): Set<Role> {
  if (seen.has(role)) return seen
  seen.add(role)
  ROLE_INHERITS[role].forEach((parent) => getEffectiveRoles(parent, seen))
  return seen
}

// للـ UX فقط (إخفاء زر/صفحة). الحماية الفعلية للبيانات على الباك
export function can(role: Role | undefined, permission: Permission): boolean {
  if (!role) return false
  for (const r of getEffectiveRoles(role)) {
    if (ROLE_PERMISSIONS[r].includes(permission)) return true
  }
  return false
}
