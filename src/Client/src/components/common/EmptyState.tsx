import type { LucideIcon } from 'lucide-react'
import { Inbox } from 'lucide-react'
import type { ReactNode } from 'react'

interface EmptyStateProps {
  title: string
  description?: string
  icon?: LucideIcon
  action?: ReactNode
}

export function EmptyState({ title, description, icon: Icon = Inbox, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 px-6 py-16 text-center">
      <span className="grid size-12 place-items-center rounded-lg bg-slate-100 text-slate-400 ring-1 ring-inset ring-slate-200">
        <Icon aria-hidden="true" className="size-6" />
      </span>
      <p className="text-sm font-semibold text-slate-700">{title}</p>
      {description && <p className="max-w-sm text-sm leading-6 text-slate-500">{description}</p>}
      {action}
    </div>
  )
}
