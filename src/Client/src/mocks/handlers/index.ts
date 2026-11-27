import { authHandlers } from './auth.handlers'
import { tasksHandlers } from './tasks.handlers'
import { contactsHandlers, usersHandlers } from './users.handlers'

// أضف handlers كل ميزة هنا عند بنائها
export const handlers = [...authHandlers, ...usersHandlers, ...contactsHandlers, ...tasksHandlers]
