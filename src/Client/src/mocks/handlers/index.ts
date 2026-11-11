import { authHandlers } from './auth.handlers'
import { usersHandlers } from './users.handlers'

// أضف handlers كل ميزة هنا عند بنائها
export const handlers = [...authHandlers, ...usersHandlers]
