import type { AppRoute } from '@/types/router'
import { CreateUserPage } from './pages/CreateUserPage'
import { UserFormPage } from './pages/UserFormPage'
import { UserDetailPage } from './pages/UserDetailPage'
import { UsersPage } from './pages/UsersPage'

export const usersRoutes: AppRoute[] = [
  {
    path: 'users',
    element: <UsersPage />,
    handle: { permission: 'users.suspend' },
  },
  {
    path: 'users/new',
    element: <CreateUserPage />,
    handle: { permission: 'users.create' },
  },
  {
    path: 'users/:userId',
    element: <UserDetailPage />,
    handle: { permission: 'users.suspend' },
  },
  {
    path: 'users/:userId/edit',
    element: <UserFormPage />,
    handle: { permission: 'users.suspend' },
  },
]
