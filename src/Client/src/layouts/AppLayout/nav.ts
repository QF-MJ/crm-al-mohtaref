import type { Permission } from '@/permissions/permissions'

export interface NavItem {
  labelKey: string // مفتاح بملف i18n/common.json
  to: string
  permission: Permission
}

// يظهر لكل مستخدم ما يملك صلاحيته فقط. الترتيب = أولوية صفحة البداية
export const navItems: NavItem[] = [
  { labelKey: 'nav.clients', to: '/clients', permission: 'clients.view' },
  { labelKey: 'nav.campaigns', to: '/campaigns', permission: 'campaigns.updateStatus' },
  { labelKey: 'nav.academic', to: '/academic/subscribed-clients', permission: 'academic.viewSubscribed' },
  { labelKey: 'nav.issues', to: '/issues', permission: 'issues.follow' },
  { labelKey: 'nav.users', to: '/users', permission: 'users.suspend' },
  { labelKey: 'nav.notifications', to: '/notifications', permission: 'notifications.view' },
]
