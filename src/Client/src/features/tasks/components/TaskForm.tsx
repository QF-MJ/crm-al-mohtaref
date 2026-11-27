import { useEffect, type ReactNode } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { AlertCircle } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select, type SelectOption } from '@/components/ui/select'
import { cn } from '@/lib/utils'
import { TASK_STATUSES } from '../taskEnums'
import {
  TASK_DESCRIPTION_MAX,
  TASK_TITLE_MAX,
  taskFormSchema,
  type TaskFormValues,
} from '../schemas/task.schema'

interface TaskFormProps {
  defaultValues: TaskFormValues
  assigneeOptions: SelectOption[]
  isPending: boolean
  errorMessage?: string
  onSubmit: (values: TaskFormValues) => void
  onCancel: () => void
}

// الحقول هنا هي فقط ما يقبله PUT /tasks/{id}: العنوان، الوصف، الحالة، البداية، النهاية، المنفذ.
export function TaskForm({
  defaultValues,
  assigneeOptions,
  isPending,
  errorMessage,
  onSubmit,
  onCancel,
}: TaskFormProps) {
  const { t } = useTranslation()
  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors },
  } = useForm<TaskFormValues>({
    resolver: zodResolver(taskFormSchema),
    defaultValues,
    mode: 'onSubmit',
  })

  useEffect(() => {
    reset(defaultValues)
  }, [defaultValues, reset])

  const statusOptions: SelectOption[] = TASK_STATUSES.map((status) => ({
    value: status.value,
    label: t(`tasks.status.${status.value}`),
  }))

  const message = (key?: string, max?: number) =>
    key ? t(key, max ? { max } : undefined) : undefined

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      noValidate
      className="space-y-5 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-7"
    >
      <div>
        <h2 className="font-semibold text-slate-900">{t('tasks.formTitle')}</h2>
        <p className="mt-1 text-xs leading-6 text-slate-500">{t('tasks.formHint')}</p>
      </div>

      {errorMessage && (
        <div
          role="alert"
          className="flex items-start gap-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800"
        >
          <AlertCircle aria-hidden="true" className="mt-0.5 size-[18px] shrink-0" />
          <p className="leading-6">{errorMessage}</p>
        </div>
      )}

      <Field
        label={t('tasks.taskTitle')}
        htmlFor="title"
        error={message(errors.title?.message, TASK_TITLE_MAX)}
      >
        <Input id="title" dir="auto" maxLength={TASK_TITLE_MAX} {...register('title')} />
      </Field>

      <Field
        label={t('tasks.description')}
        htmlFor="description"
        error={message(errors.description?.message, TASK_DESCRIPTION_MAX)}
        hint={t('tasks.descriptionHint', { max: TASK_DESCRIPTION_MAX })}
      >
        <textarea
          id="description"
          dir="auto"
          rows={4}
          maxLength={TASK_DESCRIPTION_MAX}
          className="w-full rounded-lg border border-input bg-transparent px-3 py-2 text-sm leading-7 outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
          {...register('description')}
        />
      </Field>

      <div className="grid gap-5 sm:grid-cols-2">
        <Field
          label={t('tasks.statusLabel')}
          htmlFor="status"
          error={message(errors.status?.message)}
        >
          <Controller
            name="status"
            control={control}
            render={({ field }) => (
              <Select
                id="status"
                ref={field.ref}
                onBlur={field.onBlur}
                value={field.value}
                onValueChange={field.onChange}
                options={statusOptions}
              />
            )}
          />
        </Field>

        <Field
          label={t('tasks.assignee')}
          htmlFor="assignedToUserId"
          error={message(errors.assignedToUserId?.message)}
          hint={t('tasks.assigneeHint')}
        >
          <Controller
            name="assignedToUserId"
            control={control}
            render={({ field }) => (
              <Select
                id="assignedToUserId"
                ref={field.ref}
                onBlur={field.onBlur}
                value={field.value}
                onValueChange={field.onChange}
                options={assigneeOptions}
                disabled={assigneeOptions.length <= 1}
              />
            )}
          />
        </Field>

        <Field
          label={t('tasks.startTime')}
          htmlFor="startTime"
          error={message(errors.startTime?.message)}
        >
          <Input id="startTime" type="datetime-local" {...register('startTime')} />
        </Field>

        <Field
          label={t('tasks.endTime')}
          htmlFor="endTime"
          error={message(errors.endTime?.message)}
        >
          <Input id="endTime" type="datetime-local" {...register('endTime')} />
        </Field>
      </div>

      <div className="flex flex-wrap gap-2 border-t border-slate-100 pt-5">
        <Button type="submit" disabled={isPending}>
          {isPending ? t('tasks.saving') : t('tasks.saveChanges')}
        </Button>
        <Button type="button" variant="outline" onClick={onCancel} disabled={isPending}>
          {t('actions.cancel')}
        </Button>
      </div>
    </form>
  )
}

function Field({
  label,
  htmlFor,
  error,
  hint,
  children,
}: {
  label: string
  htmlFor: string
  error?: string
  hint?: string
  children: ReactNode
}) {
  return (
    <div className="space-y-1.5">
      <label htmlFor={htmlFor} className="text-sm font-medium text-slate-700">
        {label}
      </label>
      {children}
      {error ? (
        <p role="alert" className={cn('text-xs text-destructive')}>
          {error}
        </p>
      ) : hint ? (
        <p className="text-xs leading-5 text-slate-500">{hint}</p>
      ) : null}
    </div>
  )
}
