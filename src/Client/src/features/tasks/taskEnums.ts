import type { BadgeVariant } from '@/components/ui/badge'
import type { TaskStatusValue, TaskTypeValue } from './types'

// القوائم الثابتة مطابقة لـ TaskStatus / TaskType في الباك،
// والأسماء العربية مطابقة حرفياً لـ EnumExtensions.ToTaskStatusString / ToTaskTypeString.
// الباك يرسل الاسم العربي في الردود، ويستقبل اسم التعداد الإنجليزي في الطلبات.

interface EnumEntry<TValue extends string> {
  value: TValue
  /** الاسم العربي كما يرسله الباك */
  backendName: string
}

export const TASK_STATUSES: readonly EnumEntry<TaskStatusValue>[] = [
  { value: 'Pending', backendName: 'قيد الانتظار' },
  { value: 'InProgress', backendName: 'قيد التنفيذ' },
  { value: 'Completed', backendName: 'مكتملة' },
  { value: 'Cancelled', backendName: 'ملغاة' },
  { value: 'OnHold', backendName: 'معلقة' },
]

export const TASK_TYPES: readonly EnumEntry<TaskTypeValue>[] = [
  { value: 'Normal', backendName: 'مهمة عادية' },
  { value: 'Issue', backendName: 'مشكلة' },
  { value: 'Campaign', backendName: 'حملة' },
  { value: 'Contact', backendName: 'تواصل' },
  { value: 'FollowUp', backendName: 'متابعة' },
  { value: 'Confirmation', backendName: 'تثبيت' },
  { value: 'Placement', backendName: 'تسكين' },
  { value: 'SubTask', backendName: 'مهمة فرعية' },
]

/** الباك يرفض الفلترة بنوع "مهمة فرعية" (GetTasksQueryValidator) */
export const FILTERABLE_TASK_TYPES = TASK_TYPES.filter((type) => type.value !== 'SubTask')

const normalize = (value: string) => value.trim().replace(/\s+/g, ' ')

export function statusFromName(name: string | undefined): TaskStatusValue | undefined {
  if (!name) return undefined
  return TASK_STATUSES.find((status) => normalize(status.backendName) === normalize(name))?.value
}

export function typeFromName(name: string | undefined): TaskTypeValue | undefined {
  if (!name) return undefined
  return TASK_TYPES.find((type) => normalize(type.backendName) === normalize(name))?.value
}

export const STATUS_BADGE: Record<TaskStatusValue, BadgeVariant> = {
  Pending: 'neutral',
  InProgress: 'info',
  Completed: 'success',
  Cancelled: 'danger',
  OnHold: 'warning',
}
