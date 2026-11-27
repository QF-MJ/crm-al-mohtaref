import { useTranslation } from 'react-i18next'
import { GraduationCap, Phone, UserRound } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import type { TaskContact } from '../types'

// جهة التواصل = ولي الأمر، وقد يتبعه أكثر من طالب.
// الطلاب وحالة كل طالب في الحملة يظهرون فقط حين يرسلهم الباك (سياق الحملات، وليس المشاكل).
export function TaskContactsList({ contacts }: { contacts: TaskContact[] }) {
  const { t } = useTranslation()

  if (contacts.length === 0) {
    return <p className="text-sm text-slate-500">{t('tasks.noContacts')}</p>
  }

  return (
    <ul className="space-y-3">
      {contacts.map((contact) => (
        <li key={contact.id} className="rounded-xl border border-slate-200 bg-slate-50/60 p-3">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span className="flex items-center gap-2 text-sm font-medium text-slate-800">
              <UserRound aria-hidden="true" className="size-4 text-slate-400" />
              <span dir="auto">{contact.fullName || '—'}</span>
            </span>
            <a
              href={`tel:${contact.phoneNumber}`}
              className="flex items-center gap-1.5 text-xs text-teal-700 hover:underline"
              dir="ltr"
            >
              <Phone aria-hidden="true" className="size-3.5" />
              {contact.phoneNumber}
            </a>
          </div>

          {contact.students && contact.students.length > 0 && (
            <ul className="mt-2 space-y-1.5 border-t border-slate-200 pt-2">
              {contact.students.map((student) => (
                <li
                  key={student.studentId}
                  className="flex flex-wrap items-center justify-between gap-2 text-xs"
                >
                  <span className="flex items-center gap-1.5 text-slate-600">
                    <GraduationCap aria-hidden="true" className="size-3.5 text-slate-400" />
                    <span dir="auto">{student.studentName || '—'}</span>
                  </span>
                  {student.leadStatus ? (
                    <Badge variant="info">{student.leadStatus.name}</Badge>
                  ) : (
                    <span className="text-slate-400">{t('tasks.noLeadStatus')}</span>
                  )}
                </li>
              ))}
            </ul>
          )}
        </li>
      ))}
    </ul>
  )
}
