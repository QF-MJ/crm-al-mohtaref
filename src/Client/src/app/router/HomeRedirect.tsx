import { Navigate } from 'react-router-dom'
import { navItems } from '@/layouts/AppLayout/nav'
import { usePermission } from '@/permissions/usePermission'
import { paths } from './paths'

// يوجّه المستخدم لأول صفحة يملك صلاحيتها
export function HomeRedirect() {
  const can = usePermission()
  const first = navItems.find((item) => can(item.permission))
  return <Navigate to={first?.to ?? paths.notifications} replace />
}
