interface PagePlaceholderProps {
  name: string
  fr: string
}

// صفحة مؤقتة تُستبدل بالصفحة الحقيقية عند تنفيذ الميزة بسبرينتها
export function PagePlaceholder({ name, fr }: PagePlaceholderProps) {
  return (
    <div className="rounded-lg border border-dashed p-10 text-center" dir="ltr">
      <p className="text-lg font-semibold">{name}</p>
      <p className="text-sm text-muted-foreground">{fr}</p>
    </div>
  )
}
