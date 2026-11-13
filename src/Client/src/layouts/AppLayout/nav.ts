import type { Capability } from '@/permissions/access'
import type { LucideIcon } from 'lucide-react'
import { BriefcaseBusiness, LayoutDashboard, UserRound, UsersRound } from 'lucide-react'

export interface NavItem {
  labelKey: string // مفتاح بملف i18n/common.json
  to: string
  capability: Capability
  icon: LucideIcon
}

// يظهر لكل مستخدم ما يملك صلاحيته فقط.
// الوحدات التي لم تُطلق في الباك بعد (الحملات، المشاكل، التوجيه الأكاديمي، الإشعارات)
// لا تُعرض هنا حتى لا يقود التنقل إلى شاشات فارغة.
export const navItems: NavItem[] = [
  { labelKey: 'nav.dashboard', to: '/', capability: 'account.view', icon: LayoutDashboard },
  { labelKey: 'nav.clients', to: '/clients', capability: 'contacts.view', icon: BriefcaseBusiness },
  { labelKey: 'nav.users', to: '/users', capability: 'users.view', icon: UsersRound },
  { labelKey: 'nav.account', to: '/account', capability: 'account.view', icon: UserRound },
]
