import { useEffect, useState } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { LoaderCircle } from 'lucide-react'
import { endSession, refreshAccessToken, RefreshFailedError } from '@/api/session'
import { isTokenExpired, useAuthStore } from '@/stores/auth.store'
import { paths } from '../paths'

export function RequireAuth() {
  const user = useAuthStore((state) => state.user)
  const token = useAuthStore((state) => state.token)
  const location = useLocation()
  // التوكن الذي فشل تجديده بسبب الشبكة: نعرض الصفحات بدل انتظار لا ينتهي
  const [refreshSkippedFor, setRefreshSkippedFor] = useState<string | null>(null)

  const isAuthenticated = Boolean(user && token)
  // الـ Access Token منتهٍ: نجدّده بالـ refresh token بدل إخراج المستخدم
  const needsRefresh = isAuthenticated && isTokenExpired(user) && refreshSkippedFor !== token

  useEffect(() => {
    if (!needsRefresh) return
    refreshAccessToken(token).catch((error: unknown) => {
      if (error instanceof RefreshFailedError && error.sessionEnded) {
        endSession()
      } else {
        // الخادم غير متاح: الطلبات ستحاول التجديد من جديد عند أول 401
        setRefreshSkippedFor(token)
      }
    })
  }, [needsRefresh, token])

  if (!isAuthenticated) {
    return <Navigate to={paths.login} replace state={{ from: location }} />
  }

  if (needsRefresh) return <SessionRestoring />

  return <Outlet />
}

function SessionRestoring() {
  const { t } = useTranslation()
  return (
    <div
      role="status"
      className="flex min-h-svh items-center justify-center gap-2 text-sm text-muted-foreground"
    >
      <LoaderCircle aria-hidden="true" className="size-5 animate-spin" />
      {t('session.restoring')}
    </div>
  )
}
