import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { usePermission } from '@/permissions/usePermission'
import type { Permission } from '@/permissions/permissions'
import { paths } from '../paths'

interface Props {
  permission: Permission
  children: ReactNode
}

export function RequirePermission({ permission, children }: Props) {
  const allowed = usePermission()(permission)
  return allowed ? <>{children}</> : <Navigate to={paths.forbidden} replace />
}
