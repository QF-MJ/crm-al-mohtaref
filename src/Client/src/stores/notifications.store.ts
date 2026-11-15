import { create } from 'zustand'

export interface AppNotification {
  id: string
  title: string
  body?: string
  createdAt: string
  read: boolean
}

interface NotificationsState {
  items: AppNotification[]
  markAllRead: () => void
  add: (notification: AppNotification) => void
}

// جاهزة لاستقبال إشعارات المهام من الباك لاحقاً عبر add()
export const useNotificationsStore = create<NotificationsState>()((set) => ({
  items: [],
  markAllRead: () =>
    set((state) => ({ items: state.items.map((item) => ({ ...item, read: true })) })),
  add: (notification) => set((state) => ({ items: [notification, ...state.items] })),
}))
