import type { RouteObject } from 'react-router-dom'
import type { Permission } from '@/permissions/permissions'

// كل راوت يحمل صلاحيته بـ handle، وحارس واحد (RequirePermission) يقرأها
export type AppRoute = RouteObject & { handle?: { permission?: Permission } }
