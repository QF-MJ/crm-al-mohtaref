import { useAuthStore } from '@/stores/auth.store'
import { can } from './can'
import type { Permission } from './permissions'

export function usePermission() {
  const role = useAuthStore((s) => s.user?.role)
  return (permission: Permission) => can(role, permission)
}
