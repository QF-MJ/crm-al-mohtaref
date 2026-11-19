import type { Permission } from '@/permissions/permissions'
import type { LucideIcon } from 'lucide-react'
import {
  Bell,
  BriefcaseBusiness,
  CircleHelp,
  GraduationCap,
  Megaphone,
  UsersRound,
} from 'lucide-react'

export interface NavItem {
  labelKey: string // مفتاح بملف i18n/common.json
  to: string
  permission: Permission
  icon: LucideIcon
}

// يظهر لكل مستخدم ما يملك صلاحيته فقط. الترتيب = أولوية صفحة البداية
export const navItems: NavItem[] = [
  { labelKey: 'nav.clients', to: '/clients', permission: 'clients.view', icon: BriefcaseBusiness },
  {
    labelKey: 'nav.campaigns',
    to: '/campaigns',
    permission: 'campaigns.updateStatus',
    icon: Megaphone,
  },
  {
    labelKey: 'nav.academic',
    to: '/academic/subscribed-clients',
    permission: 'academic.viewSubscribed',
    icon: GraduationCap,
  },
  { labelKey: 'nav.issues', to: '/issues', permission: 'issues.follow', icon: CircleHelp },
  { labelKey: 'nav.users', to: '/users', permission: 'users.suspend', icon: UsersRound },
  { labelKey: 'nav.notifications', to: '/notifications', permission: 'notifications.view', icon: Bell },
]
