import { useTranslation } from 'react-i18next'
import { Badge } from '@/components/ui/badge'
import { STATUS_BADGE, statusFromName } from '../taskEnums'

// الباك يرسل الأسماء بالعربية. نحوّلها لقيمة التعداد لنترجمها ونلوّنها،
// وإن ظهر اسم غير معروف نعرضه كما هو بدل إخفائه.

export function TaskStatusBadge({ statusName }: { statusName: string }) {
  const { t } = useTranslation()
  const status = statusFromName(statusName)
  return (
    <Badge variant={status ? STATUS_BADGE[status] : 'neutral'} dot>
      {status ? t(`tasks.status.${status}`) : statusName}
    </Badge>
  )
}
