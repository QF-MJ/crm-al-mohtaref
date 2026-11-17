import { ROLE_INHERITS, type Role } from './roles'
import { ROLE_PERMISSIONS, type Permission } from './permissions'

// الدور + كل الأدوار التي يرثها (بشكل متسلسل)
export function getEffectiveRoles(role: Role, seen = new Set<Role>()): Set<Role> {
  if (!Object.prototype.hasOwnProperty.call(ROLE_INHERITS, role) || seen.has(role)) return seen
  const inheritedRoles = ROLE_INHERITS[role]
  seen.add(role)
  inheritedRoles.forEach((parent) => getEffectiveRoles(parent, seen))
  return seen
}

// للـ UX فقط (إخفاء زر/صفحة). الحماية الفعلية للبيانات على الباك
export function can(roles: Role[] | undefined, permission: Permission): boolean {
  if (!roles?.length) return false
  for (const role of roles) {
    for (const effectiveRole of getEffectiveRoles(role)) {
      if (ROLE_PERMISSIONS[effectiveRole]?.includes(permission)) return true
    }
  }
  return false
}

// المستخدم قد يملك أكثر من دور (قسم + دور لكل صف). يكفي أن يسمح به أحدها
export function canAny(roles: readonly Role[], permission: Permission): boolean {
  return roles.some((role) => can([role], permission))
}
