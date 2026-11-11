import { useAuthStore } from '@/stores/auth.store'
import { can } from './can'
import type { Permission } from './permissions'
import type { Role } from './roles'

export function usePermission() {
<<<<<<< Updated upstream
  const roles = useAuthStore((s) => s.user?.roles)
  return (permission: Permission) => can(roles, permission)
=======
  const pairs = useAuthStore((s) => s.user?.permissions)
  const accountName = useAuthStore((s) => s.user?.accountName)
  const roles = useMemo<Role[]>(() => {
    const mappedRoles = mapRoles(pairs ?? [])
    // The seeded backend admin has no permission rows; this only restores frontend navigation.
    if (accountName?.trim().toLowerCase() === 'admin') mappedRoles.push('admin')
    return mappedRoles
  }, [pairs, accountName])
  return (permission: Permission) => canAny(roles, permission)
>>>>>>> Stashed changes
}
