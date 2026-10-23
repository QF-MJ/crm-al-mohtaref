import { Navigate, Outlet } from 'react-router-dom'
import { useAuthStore } from '@/stores/auth.store'

export function AuthLayout() {
  const user = useAuthStore((s) => s.user)
  if (user) return <Navigate to="/" replace />
  return (
    <main className="grid min-h-screen place-items-center bg-muted/40 p-4">
      <Outlet />
    </main>
  )
}
