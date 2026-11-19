import { useTranslation } from 'react-i18next'
import { Badge } from '@/components/ui/badge'
import { useAccess } from '@/permissions/useAccess'

// ملخص مرئي لهوية المستخدم: مدير نظام / مدير قسم / موظف
export function AccessBadge() {
  const { t } = useTranslation()
  const access = useAccess()

  if (access.pairs.length === 0) {
    return <Badge variant="warning">{t('permissions.none')}</Badge>
  }

  if (access.isAdmin) {
    return (
      <Badge variant="success" dot>
        {t('permissions.admin')}
      </Badge>
    )
  }

  const primary = access.pairs[0]
  return (
    <Badge variant={access.isManager ? 'info' : 'neutral'} dot>
      {primary.roleName} · {primary.departmentName}
    </Badge>
  )
}
