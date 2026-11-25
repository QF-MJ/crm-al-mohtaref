import { useTranslation } from 'react-i18next'
import { Badge, type BadgeVariant } from '@/components/ui/badge'

// ألوان حالات الحملة حسب أسماء SeedRules في الباك.
// أي حالة جديدة غير معروفة تظهر بلون محايد بدل أن تكسر الصفحة.
const STATUS_VARIANTS: Record<string, BadgeVariant> = {
  مسودة: 'neutral',
  نشطة: 'success',
  'متوقفة مؤقتاً': 'warning',
  منتهية: 'info',
  ملغاة: 'danger',
}

export function CampaignStatusBadge({ name }: { name?: string }) {
  const { t } = useTranslation()
  if (!name) return <span className="text-xs text-slate-400">{t('campaigns.unknownStatus')}</span>

  return (
    <Badge variant={STATUS_VARIANTS[name.trim()] ?? 'neutral'} dot>
      {name}
    </Badge>
  )
}
