import type { AppRoute } from '@/types/router'
import { ApiUnavailablePage } from '@/components/common/ApiUnavailablePage'
import { useTranslation } from 'react-i18next'
import { ContactsPage } from './pages/ContactsPage'

function ContactsImportUnavailablePage() {
  const { t } = useTranslation()
  return <ApiUnavailablePage title={t('contacts.importTitle')} description={t('contacts.importUnavailable')} />
}

function ContactProfileUnavailablePage() {
  const { t } = useTranslation()
  return <ApiUnavailablePage title={t('contacts.profileTitle')} description={t('contacts.profileUnavailable')} />
}

export const clientsRoutes: AppRoute[] = [
  {
    path: 'clients',
    element: <ContactsPage />,
    handle: { permission: 'clients.view' },
  },
  {
    path: 'clients/import',
    element: <ContactsImportUnavailablePage />,
    handle: { permission: 'clients.import' },
  },
  {
    path: 'clients/:clientId',
    element: <ContactProfileUnavailablePage />,
    handle: { permission: 'clients.viewProfile' },
  },
]
