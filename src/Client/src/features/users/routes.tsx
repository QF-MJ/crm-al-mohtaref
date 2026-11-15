import type { AppRoute } from '@/types/router'
import { AccountPage } from './pages/AccountPage'
import { CreateUserPage } from './pages/CreateUserPage'
import { UserDetailPage } from './pages/UserDetailPage'
import { UserFormPage } from './pages/UserFormPage'
import { UsersPage } from './pages/UsersPage'

export const usersRoutes: AppRoute[] = [
  {
    path: 'account',
    element: <AccountPage />,
    handle: { capability: 'account.view' },
  },
  {
    path: 'users',
    element: <UsersPage />,
    handle: { capability: 'users.view' },
  },
  {
    path: 'users/new',
    element: <CreateUserPage />,
    handle: { capability: 'users.create' },
  },
  {
    path: 'users/:userId',
    element: <UserDetailPage />,
    handle: { capability: 'users.view' },
  },
  {
    path: 'users/:userId/edit',
    element: <UserFormPage />,
    handle: { capability: 'users.update' },
  },
]
