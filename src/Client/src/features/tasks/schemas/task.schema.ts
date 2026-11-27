import { z } from 'zod'
import { parseApiDate } from '../lib/dates'

// نفس قواعد UpdateTaskCommandValidator في الباك. الرسائل مفاتيح ترجمة.
export const TASK_TITLE_MAX = 200
export const TASK_DESCRIPTION_MAX = 2000

const STATUS_VALUES = ['Pending', 'InProgress', 'Completed', 'Cancelled', 'OnHold'] as const

const dateTimeField = z
  .string()
  .min(1, 'validation.required')
  .refine((value) => parseApiDate(value) !== undefined, 'validation.invalidDate')

export const taskFormSchema = z
  .object({
    title: z
      .string()
      .trim()
      .min(1, 'validation.required')
      .max(TASK_TITLE_MAX, 'validation.maxLength'),
    description: z.string().trim().max(TASK_DESCRIPTION_MAX, 'validation.maxLength'),
    status: z.enum(STATUS_VALUES, { message: 'validation.required' }),
    startTime: dateTimeField,
    endTime: dateTimeField,
    assignedToUserId: z.string().min(1, 'validation.required'),
  })
  // الـ yaml: النهاية بعد البداية أو تساويها (الباك أكثر تساهلاً فيقارن بتاريخ البداية فقط)
  .refine(
    (values) => {
      const start = parseApiDate(values.startTime)
      const end = parseApiDate(values.endTime)
      return !start || !end || end.getTime() >= start.getTime()
    },
    { path: ['endTime'], message: 'validation.endBeforeStart' },
  )

export type TaskFormValues = z.infer<typeof taskFormSchema>
