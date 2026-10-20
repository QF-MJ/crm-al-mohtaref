import type { AppRoute } from '@/types/router'
import { PagePlaceholder } from '@/components/common/PagePlaceholder'

export const clientsRoutes: AppRoute[] = [
  {
    path: 'clients',
    element: <PagePlaceholder name="ClientsList" fr="FR-07 FR-08 FR-09" />,
    handle: { permission: 'clients.view' },
  },
  {
    path: 'clients/import',
    element: <PagePlaceholder name="ClientsImport" fr="FR-06" />,
    handle: { permission: 'clients.import' },
  },
  {
    path: 'clients/:clientId',
    element: <PagePlaceholder name="ClientProfile" fr="FR-10" />,
    handle: { permission: 'clients.viewProfile' },
  },
]
