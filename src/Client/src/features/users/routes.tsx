import type { AppRoute } from '@/types/router'
import { UserFormPage } from './pages/UserFormPage'
import { UsersPage } from './pages/UsersPage'

export const usersRoutes: AppRoute[] = [
  {
    path: 'users',
    element: <UsersPage />,
    handle: { permission: 'users.suspend' },
  },
  {
    path: 'users/:userId/edit',
    element: <UserFormPage />,
    handle: { permission: 'users.suspend' },
  },
]
