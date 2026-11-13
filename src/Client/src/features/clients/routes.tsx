import type { AppRoute } from '@/types/router'
import { UnavailableRoute } from '@/components/common/UnavailableRoute'
import { ContactsPage } from './pages/ContactsPage'

export const clientsRoutes: AppRoute[] = [
  {
    path: 'clients',
    element: <ContactsPage />,
    handle: { capability: 'contacts.view' },
  },
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
  {
    path: 'clients/:clientId',
    element: (
      <UnavailableRoute
        titleKey="contacts.profileTitle"
        descriptionKey="contacts.profileUnavailable"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
]
