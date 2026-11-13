import type { ReactNode } from 'react'
import { useAccess } from './useAccess'
import type { Capability } from './access'

interface CanProps {
  capability: Capability
  children: ReactNode
  fallback?: ReactNode
}

// مثال: <Can capability="users.create"><Link …/></Can>
export function Can({ capability, children, fallback = null }: CanProps) {
  const access = useAccess()
  return <>{access.can(capability) ? children : fallback}</>
}
