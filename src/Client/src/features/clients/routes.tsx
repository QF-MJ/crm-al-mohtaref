import type { AppRoute } from '@/types/router'
import { UnavailableRoute } from '@/components/common/UnavailableRoute'
import { ContactsPage } from './pages/ContactsPage'
import { CustomerProfilePage } from './pages/CustomerProfilePage'

export const clientsRoutes: AppRoute[] = [
  {
    path: 'clients',
    element: <ContactsPage />,
    handle: { capability: 'contacts.view' },
  },
  {
    path: 'clients/:clientId',
    element: <CustomerProfilePage />,
    handle: { capability: 'contacts.view' },
  },
]

// صفحات لم تُطلق في الباك بعد: تُسجّل في وضع التطوير فقط (انظر app/router/index.tsx)
export const clientsPreviewRoutes: AppRoute[] = [
  {
    path: 'clients/import',
    element: (
      <UnavailableRoute
        titleKey="contacts.importTitle"
        descriptionKey="contacts.importUnavailable"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
]
