import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'

export type Theme = 'light' | 'dark'

export const THEME_STORAGE_KEY = 'crm-theme'

const prefersDark = () =>
  typeof window !== 'undefined' &&
  typeof window.matchMedia === 'function' &&
  window.matchMedia('(prefers-color-scheme: dark)').matches

// نطبّق الثيم على <html> لأن متغيّرات index.css / brand.css معرّفة على .dark
export function applyTheme(theme: Theme) {
  if (typeof document === 'undefined') return
  const root = document.documentElement
  root.classList.toggle('dark', theme === 'dark')
  root.style.colorScheme = theme
}

interface ThemeState {
  theme: Theme
  setTheme: (theme: Theme) => void
  toggle: () => void
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set, get) => ({
      // الافتراضي = تفضيل النظام، ويُستبدل بالمحفوظ إن وُجد
      theme: prefersDark() ? 'dark' : 'light',
      setTheme: (theme) => {
        applyTheme(theme)
        set({ theme })
      },
      toggle: () => get().setTheme(get().theme === 'dark' ? 'light' : 'dark'),
    }),
    {
      name: THEME_STORAGE_KEY,
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({ theme: state.theme }),
    },
  ),
)

// مزامنة فورية بعد إعادة التحميل (index.html يطبّقها قبل الرسم لتفادي الوميض)
applyTheme(useThemeStore.getState().theme)
