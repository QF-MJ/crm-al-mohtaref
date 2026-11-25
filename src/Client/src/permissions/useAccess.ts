import { useMemo } from 'react'
import { useAuthStore } from '@/stores/auth.store'
import { buildAccess, type Access } from './access'

// يبني نموذج الوصول مرة واحدة لكل تغيير بالجلسة
export function useAccess(): Access {
  const pairs = useAuthStore((state) => state.user?.permissions)
  return useMemo(() => buildAccess(pairs ?? []), [pairs])
}
