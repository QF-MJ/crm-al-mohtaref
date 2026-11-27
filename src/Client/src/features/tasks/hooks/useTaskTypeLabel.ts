import { useTranslation } from 'react-i18next'
import { typeFromName } from '../taskEnums'

// الباك يرسل اسم النوع بالعربية؛ نحوّله لقيمة التعداد لنترجمه، أو نعرضه كما هو إن كان غير معروف
export function useTaskTypeLabel() {
  const { t } = useTranslation()
  return (typeName: string | undefined) => {
    if (!typeName) return '—'
    const type = typeFromName(typeName)
    return type ? t(`tasks.type.${type}`) : typeName
  }
}
