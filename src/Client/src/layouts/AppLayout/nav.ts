import type { Capability } from '@/permissions/access'
import type { LucideIcon } from 'lucide-react'
import {
  BriefcaseBusiness,
  LayoutDashboard,
  ListChecks,
  Megaphone,
  UserRound,
  UsersRound,
} from 'lucide-react'

export interface NavItem {
  labelKey: string // مفتاح بملف i18n/common.json
  to: string
  capability: Capability
  icon: LucideIcon
}

// يظهر لكل مستخدم ما يملك صلاحيته فقط.
export const navItems: NavItem[] = [
  { labelKey: 'nav.dashboard', to: '/', capability: 'account.view', icon: LayoutDashboard },
  { labelKey: 'nav.tasks', to: '/tasks', capability: 'tasks.view', icon: ListChecks },
  { labelKey: 'nav.clients', to: '/clients', capability: 'contacts.view', icon: BriefcaseBusiness },
  {
    labelKey: 'nav.campaigns',
    to: '/campaigns',
    capability: 'campaigns.view',
    icon: Megaphone,
  },
  { labelKey: 'nav.users', to: '/users', capability: 'users.view', icon: UsersRound },
  { labelKey: 'nav.account', to: '/account', capability: 'account.view', icon: UserRound },
]
