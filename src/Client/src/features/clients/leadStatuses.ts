export type LeadStatusValue = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10

export interface LeadStatusOption {
  value: LeadStatusValue
  label: string
}

// حالات العميل ثابتة (enum) ولم تعد تُجلب من الـ API. القيم/التسميات تطابق الباك.
export const LEAD_STATUSES: readonly LeadStatusOption[] = [
  { value: 1, label: 'جديد' },
  { value: 2, label: 'قيد التواصل' },
  { value: 3, label: 'فشل التواصل' },
  { value: 4, label: 'حدثت استجابة' },
  { value: 5, label: 'قيد المتابعة' },
  { value: 6, label: 'فشلت المتابعة' },
  { value: 7, label: 'قيد التثبيت' },
  { value: 8, label: 'تم التثبيت' },
  { value: 9, label: 'قيد التسكين' },
  { value: 10, label: 'تم التسكين' },
] as const

export const LEAD_STATUS_OPTIONS = LEAD_STATUSES.map((status) => ({
  value: String(status.value),
  label: status.label,
}))

export function leadStatusLabel(value: number | string | undefined): string | undefined {
  if (value === undefined) return undefined
  const numeric = typeof value === 'string' ? Number(value) : value
  return LEAD_STATUSES.find((status) => status.value === numeric)?.label
}
