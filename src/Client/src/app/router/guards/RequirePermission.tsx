import type { ReactNode } from 'react'
import { useAccess } from '@/permissions/useAccess'
import type { Capability } from '@/permissions/access'
import { ForbiddenPage } from '../pages/ForbiddenPage'

interface Props {
  capability: Capability
  children: ReactNode
}

// نعرض صفحة "غير مصرّح" داخل التخطيط نفسه بدل تحويل المستخدم إلى صفحة منفصلة
export function RequirePermission({ capability, children }: Props) {
  const access = useAccess()
  return access.can(capability) ? <>{children}</> : <ForbiddenPage />
}
