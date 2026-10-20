import type { AppRoute } from '@/types/router'
import { PagePlaceholder } from '@/components/common/PagePlaceholder'

export const authRoutes: AppRoute[] = [
  {
    path: 'account/password',
    element: <PagePlaceholder name="AccountPassword" fr="FR-02" />,
    handle: { permission: 'account.changePassword' },
  },
]
