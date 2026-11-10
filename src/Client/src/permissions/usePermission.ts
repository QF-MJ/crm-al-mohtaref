import { useAuthStore } from '@/stores/auth.store'
import { can } from './can'
import type { Permission } from './permissions'

export function usePermission() {
  const roles = useAuthStore((s) => s.user?.roles)
  return (permission: Permission) => can(roles, permission)
}
