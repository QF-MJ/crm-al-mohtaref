import { QueryClientProvider } from '@tanstack/react-query'
import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Toaster } from 'sonner'
import { queryClient } from '@/lib/queryClient'
import { useThemeStore } from '@/stores/theme.store'
import '@/i18n'

// اتجاه التنبيهات ومظهرها يتبعان الواجهة الحالية
function AppToaster() {
  const { i18n } = useTranslation()
  const theme = useThemeStore((state) => state.theme)

  return (
    <Toaster
      richColors
      closeButton
      position="top-center"
      theme={theme}
      dir={i18n.dir() === 'ltr' ? 'ltr' : 'rtl'}
    />
  )
}

export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <QueryClientProvider client={queryClient}>
      {children}
      <AppToaster />
    </QueryClientProvider>
  )
}
