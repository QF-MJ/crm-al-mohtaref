import { useEffect, useRef, useState } from 'react'
import { Bell, CheckCheck } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { cn } from '@/lib/utils'
import { useNotificationsStore } from '@/stores/notifications.store'

// جرس الإشعارات: متاح لكل المستخدمين وفي كل الصفحات
export function NotificationsMenu() {
  const { t, i18n } = useTranslation()
  const items = useNotificationsStore((state) => state.items)
  const markAllRead = useNotificationsStore((state) => state.markAllRead)
  const [open, setOpen] = useState(false)
  const rootRef = useRef<HTMLDivElement>(null)

  const unread = items.filter((item) => !item.read).length
  const formatTime = (value: string) => {
    try {
      return new Intl.DateTimeFormat(i18n.language, {
        dateStyle: 'short',
        timeStyle: 'short',
      }).format(new Date(value))
    } catch {
      return value
    }
  }

  useEffect(() => {
    if (!open) return

    const handlePointerDown = (event: PointerEvent) => {
      if (event.target instanceof Node && !rootRef.current?.contains(event.target)) setOpen(false)
    }
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false)
    }

    document.addEventListener('pointerdown', handlePointerDown)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('pointerdown', handlePointerDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [open])

  return (
    <div ref={rootRef} className="relative">
      <button
        type="button"
        onClick={() => setOpen((value) => !value)}
        aria-expanded={open}
        aria-haspopup="dialog"
        aria-label={
          unread > 0 ? t('notifications.unread', { count: unread }) : t('notifications.open')
        }
        className="relative grid size-9 place-items-center rounded-lg text-slate-600 transition-colors hover:bg-slate-100 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/40"
      >
        <Bell aria-hidden="true" className="size-5" />
        {unread > 0 && (
          <span
            aria-hidden="true"
            className="absolute -top-0.5 -end-0.5 grid h-4 min-w-4 place-items-center rounded-full bg-destructive px-1 text-[10px] font-bold text-white"
          >
            {unread > 9 ? '9+' : unread}
          </span>
        )}
      </button>

      {open && (
        <div
          role="dialog"
          aria-label={t('notifications.title')}
          className="absolute end-0 top-[calc(100%+1.25rem)] z-40 w-[min(20rem,calc(100vw-2rem))] overflow-hidden rounded-xl border border-slate-200 bg-white shadow-lg shadow-slate-900/5"
        >
          <header className="flex items-center justify-between gap-2 border-b border-slate-100 px-4 py-3">
            <p className="text-sm font-semibold text-slate-900">{t('notifications.title')}</p>
            {unread > 0 && (
              <button
                type="button"
                onClick={markAllRead}
                className="inline-flex items-center gap-1 text-xs font-medium text-teal-700 hover:underline"
              >
                <CheckCheck aria-hidden="true" className="size-3.5" />
                {t('notifications.markAllRead')}
              </button>
            )}
          </header>

          {items.length === 0 ? (
            <div className="px-5 py-9 text-center">
              <span className="mx-auto grid size-10 place-items-center rounded-full bg-slate-100 text-slate-400">
                <Bell aria-hidden="true" className="size-5" />
              </span>
              <p className="mt-3 text-sm font-medium text-slate-700">{t('notifications.empty')}</p>
              <p className="mt-1 text-xs leading-6 text-slate-500">
                {t('notifications.emptyHint')}
              </p>
            </div>
          ) : (
            <ul className="max-h-80 divide-y divide-slate-100 overflow-y-auto">
              {items.map((item) => (
                <li key={item.id} className={cn('px-4 py-3', !item.read && 'bg-teal-50/40')}>
                  <p className="text-sm font-medium text-slate-800">{item.title}</p>
                  {item.body && (
                    <p className="mt-0.5 text-xs leading-6 text-slate-500">{item.body}</p>
                  )}
                  <p className="mt-1 text-[11px] text-slate-400">{formatTime(item.createdAt)}</p>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  )
}
