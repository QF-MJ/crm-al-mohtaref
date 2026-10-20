import { createBrowserRouter, type RouteObject } from 'react-router-dom'
import { AuthLayout } from '@/layouts/AuthLayout'
import { AppLayout } from '@/layouts/AppLayout/AppLayout'
import type { AppRoute } from '@/types/router'
// الاستيراد من index.ts للميزة فقط (قاعدة الحدود)
import { authRoutes, LoginPage } from '@/features/auth'
import { notificationsRoutes } from '@/features/notifications'
import { usersRoutes } from '@/features/users'
import { clientsRoutes } from '@/features/clients'
import { communicationsRoutes } from '@/features/communications'
import { campaignsRoutes } from '@/features/campaigns'
import { academicRoutes } from '@/features/academic'
import { issuesRoutes } from '@/features/issues'
import { RequireAuth } from './guards/RequireAuth'
import { RequirePermission } from './guards/RequirePermission'
import { HomeRedirect } from './HomeRedirect'
import { ForbiddenPage } from './pages/ForbiddenPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { paths } from './paths'

// أضف مسارات الميزة الجديدة هنا (سطر واحد لكل ميزة)
const featureRoutes: AppRoute[] = [
  ...authRoutes,
  ...notificationsRoutes,
  ...usersRoutes,
  ...clientsRoutes,
  ...communicationsRoutes,
  ...campaignsRoutes,
  ...academicRoutes,
  ...issuesRoutes,
]

// يغلّف كل راوت له صلاحية بالحارس تلقائياً، فلا ifs متفرقة بالصفحات
function protect(routes: AppRoute[]): RouteObject[] {
  return routes.map((route) => {
    const permission = route.handle?.permission
    if (!permission) return route
    return {
      ...route,
      element: <RequirePermission permission={permission}>{route.element}</RequirePermission>,
    } as RouteObject
  })
}

export const router = createBrowserRouter([
  { element: <AuthLayout />, children: [{ path: paths.login, element: <LoginPage /> }] },
  {
    element: <RequireAuth />,
    children: [
      {
        element: <AppLayout />,
        children: [{ index: true, element: <HomeRedirect /> }, ...protect(featureRoutes)],
      },
    ],
  },
  { path: paths.forbidden, element: <ForbiddenPage /> },
  { path: '*', element: <NotFoundPage /> },
])
