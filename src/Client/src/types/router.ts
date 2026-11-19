import type { RouteObject } from 'react-router-dom'
import type { Capability } from '@/permissions/access'

// كل راوت يحمل قدرته بـ handle، وحارس واحد (RequirePermission) يقرأها
export type AppRoute = RouteObject & { handle?: { capability?: Capability } }
