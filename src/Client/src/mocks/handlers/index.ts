import { authHandlers } from './auth.handlers'
import { contactsHandlers, usersHandlers } from './users.handlers'

// أضف handlers كل ميزة هنا عند بنائها
export const handlers = [...authHandlers, ...usersHandlers, ...contactsHandlers]
