import { Moon, Sun } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'
import { useThemeStore } from '@/stores/theme.store'

// زر موحّد لتبديل الوضع الفاتح/الليلي، يُستخدم في الترويسة وشاشة الدخول
export function ThemeToggle({ className }: { className?: string }) {
  const { t } = useTranslation()
  const theme = useThemeStore((state) => state.theme)
  const toggle = useThemeStore((state) => state.toggle)
  const label = theme === 'dark' ? t('actions.lightMode') : t('actions.darkMode')

  return (
    <Button
      type="button"
      variant="ghost"
      size="icon"
      onClick={toggle}
      aria-label={label}
      title={label}
      className={cn('text-slate-600', className)}
    >
      {theme === 'dark' ? <Sun aria-hidden="true" /> : <Moon aria-hidden="true" />}
    </Button>
  )
}
