// الباك يخزّن DateTime ويرسله بلا منطقة زمنية ("2026-01-18T00:00:00").
// نتعامل معه كوقت محلي كما هو (وقت الحائط)، ونرسله بنفس الصيغة دون Z،
// حتى لا يتحرك الموعد بسبب فرق التوقيت بين الجهاز والخادم.

const pad = (value: number) => String(value).padStart(2, '0')

/** يحوّل نص الباك إلى Date محلي. يتجاهل أي منطقة زمنية إن وُجدت. */
export function parseApiDate(value: string | undefined): Date | undefined {
  if (!value) return undefined
  const match = /^(\d{4})-(\d{2})-(\d{2})(?:T(\d{2}):(\d{2})(?::(\d{2}))?)?/.exec(value)
  if (!match) return undefined
  const [, year, month, day, hour = '0', minute = '0', second = '0'] = match
  const date = new Date(
    Number(year),
    Number(month) - 1,
    Number(day),
    Number(hour),
    Number(minute),
    Number(second),
  )
  return Number.isNaN(date.getTime()) ? undefined : date
}

/** Date → "YYYY-MM-DDTHH:mm:ss" بالوقت المحلي (الصيغة التي يرسلها ويستقبلها الباك) */
export function toApiDate(date: Date): string {
  return `${toDateInput(date)}T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`
}

/** Date → قيمة <input type="date"> */
export function toDateInput(date: Date): string {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

/** Date → قيمة <input type="datetime-local"> */
export function toDateTimeInput(date: Date): string {
  return `${toDateInput(date)}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

/** نص الباك → قيمة <input type="datetime-local"> */
export function apiDateToInput(value: string | undefined): string {
  const date = parseApiDate(value)
  return date ? toDateTimeInput(date) : ''
}

/** قيمة <input type="datetime-local"> → نص الباك */
export function inputToApiDate(value: string): string {
  const date = parseApiDate(value)
  return date ? toApiDate(date) : value
}

/** بداية اليوم/نهايته لقيمة <input type="date"> */
export function dayStart(dateInput: string): string {
  return `${dateInput}T00:00:00`
}

export function dayEnd(dateInput: string): string {
  return `${dateInput}T23:59:59`
}

export function addMonths(date: Date, months: number): Date {
  const result = new Date(date)
  result.setMonth(result.getMonth() + months)
  return result
}

export function formatDateTime(value: string | undefined, locale: string): string {
  const date = parseApiDate(value)
  if (!date) return '—'
  return new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(date)
}

export function formatDate(value: string | undefined, locale: string): string {
  const date = parseApiDate(value)
  if (!date) return '—'
  return new Intl.DateTimeFormat(locale, { dateStyle: 'medium' }).format(date)
}
