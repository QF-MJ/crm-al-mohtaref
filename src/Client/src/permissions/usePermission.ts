import { useMemo } from 'react'
import { useAuthStore } from '@/stores/auth.store'
import { canAny } from './can'
import { mapRoles } from './mapRoles'
import type { Permission } from './permissions'

export function usePermission() {
  const pairs = useAuthStore((s) => s.user?.permissions)
  const roles = useMemo(() => mapRoles(pairs ?? []), [pairs])
  return (permission: Permission) => canAny(roles, permission)
}
