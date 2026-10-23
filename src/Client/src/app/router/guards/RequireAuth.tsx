import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/stores/auth.store'
import { paths } from '../paths'

export function RequireAuth() {
  const user = useAuthStore((s) => s.user)
  const location = useLocation()
  if (!user) return <Navigate to={paths.login} replace state={{ from: location }} />
  return <Outlet />
}
