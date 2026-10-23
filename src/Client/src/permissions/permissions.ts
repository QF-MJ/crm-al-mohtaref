import type { Role } from './roles'

export type Permission =
  | 'account.changePassword' // FR-02
  | 'notifications.view' // FR-03
  | 'clients.viewProfile' // FR-10
  | 'clients.view' // FR-09
  | 'clients.import' // FR-06
  | 'clients.filter' // FR-07
  | 'clients.distribute' // FR-08
  | 'communications.manage' // FR-11, FR-12
  | 'campaigns.create' // FR-13
  | 'campaigns.updateStatus' // FR-14
  | 'campaigns.reports' // FR-15..17
  | 'academic.viewSubscribed' // FR-18, FR-19
  | 'academic.records' // FR-20
  | 'issues.create' // FR-21
  | 'issues.escalate' // FR-22
  | 'issues.follow' // FR-23
  | 'users.create' // FR-04
  | 'users.suspend' // FR-05

// كل دور يعرّف صلاحياته الخاصة فقط. الموروث يُجمع تلقائياً (can.ts)
export const ROLE_PERMISSIONS: Record<Role, Permission[]> = {
  employee: ['account.changePassword', 'notifications.view', 'clients.viewProfile'],
  salesEmployee: ['clients.view', 'communications.manage'],
  salesManager: ['clients.import', 'clients.filter', 'clients.distribute'],
  marketingEmployee: ['clients.view'],
  marketingManager: ['campaigns.create', 'campaigns.updateStatus', 'campaigns.reports'],
  academicAdvisor: ['academic.viewSubscribed', 'academic.records'],
  customerService: ['issues.create', 'issues.escalate', 'issues.follow'],
  admin: ['users.create', 'users.suspend'],
}
