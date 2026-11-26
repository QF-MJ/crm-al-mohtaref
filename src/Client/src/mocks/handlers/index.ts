import { authHandlers, mockAuthGate } from './auth.handlers'
import { contactsHandlers, usersHandlers } from './users.handlers'

// أضف handlers كل ميزة هنا عند بنائها.
// mockAuthGate أولاً: يرفض بـ401 أي طلب بلا Access Token صالح قبل وصوله لباقي الـ handlers.
export const handlers = [mockAuthGate, ...authHandlers, ...usersHandlers, ...contactsHandlers]
