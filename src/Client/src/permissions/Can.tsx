import type { ReactNode } from 'react'
import { usePermission } from './usePermission'
import type { Permission } from './permissions'

interface CanProps {
  permission: Permission
  children: ReactNode
  fallback?: ReactNode
}

// مثال: <Can permission="clients.distribute"><Button>توزيع</Button></Can>
export function Can({ permission, children, fallback = null }: CanProps) {
  const allowed = usePermission()(permission)
  return <>{allowed ? children : fallback}</>
}
