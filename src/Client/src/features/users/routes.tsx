import type { AppRoute } from '@/types/router'
import { PagePlaceholder } from '@/components/common/PagePlaceholder'

export const usersRoutes: AppRoute[] = [
  {
    path: 'users',
    element: <PagePlaceholder name="Users" fr="FR-05" />,
    handle: { permission: 'users.suspend' },
  },
  {
    path: 'users/new',
    element: <PagePlaceholder name="UsersNew" fr="FR-04" />,
    handle: { permission: 'users.create' },
  },
]
