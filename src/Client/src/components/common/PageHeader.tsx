import type { ReactNode } from 'react'

interface PageHeaderProps {
  title: string
  description?: string
  breadcrumb?: string
  actions?: ReactNode
}

// ترويسة الصفحة تعمل كـ «رأس مستند»: مسار صغير بخط إشارة، عنوان واضح،
// ووصف محدود العرض حتى لا يتمدد النص العربي على كامل الشاشة.
export function PageHeader({ title, description, breadcrumb, actions }: PageHeaderProps) {
  return (
    <header className="flex flex-col gap-4 border-b border-slate-200 pb-4 sm:flex-row sm:items-end sm:justify-between">
      <div className="min-w-0">
        {breadcrumb && (
          <div className="mb-2 flex items-center gap-2">
            <span aria-hidden="true" className="h-3 w-0.5 rounded-full bg-primary" />
            <p className="text-xs font-medium text-slate-500">{breadcrumb}</p>
          </div>
        )}
        <h1 className="text-2xl font-bold leading-tight text-slate-900 sm:text-[1.75rem]">
          {title}
        </h1>
        {description && (
          <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-500">{description}</p>
        )}
      </div>
      {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
    </header>
  )
}
