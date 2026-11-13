import { useEffect } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { hasActiveSession, useAuthStore } from '@/stores/auth.store'
import { paths } from '../paths'

export function RequireAuth() {
  const user = useAuthStore((state) => state.user)
  const token = useAuthStore((state) => state.token)
  const logout = useAuthStore((state) => state.logout)
  const location = useLocation()

  const isAuthenticated = Boolean(user && token)
  // التوكن منتهٍ = جلسة غير صالحة، نمسحها بدل إرسال طلبات سترفض بـ401
  const expired = isAuthenticated && !hasActiveSession({ user, token })

  useEffect(() => {
    if (expired) logout()
  }, [expired, logout])

  if (!isAuthenticated || expired) {
    return <Navigate to={paths.login} replace state={{ from: location }} />
  }

  return <Outlet />
}
