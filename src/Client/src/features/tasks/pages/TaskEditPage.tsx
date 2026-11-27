import { useMemo, useState, type ReactNode } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { Lock, ShieldAlert } from 'lucide-react'
import type { SelectOption } from '@/components/ui/select'
import { PageHeader } from '@/components/common/PageHeader'
import { ErrorState } from '@/components/common/ErrorState'
import { paths } from '@/app/router/paths'
import { describeApiError } from '@/api/errors'
import { useAccess } from '@/permissions/useAccess'
import { useAuthStore } from '@/stores/auth.store'
import { useManagedUsers } from '@/features/lookups'
import { useTask, useUpdateTask } from '../api/tasks.queries'
import { statusFromName } from '../taskEnums'
import { TaskForm } from '../components/TaskForm'
import { useTaskTypeLabel } from '../hooks/useTaskTypeLabel'
import { canEditTask } from '../lib/taskAccess'
import { apiDateToInput, inputToApiDate } from '../lib/dates'
import type { TaskFormValues } from '../schemas/task.schema'

export function TaskEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { taskId } = useParams<{ taskId: string }>()
  const access = useAccess()
  const sessionUser = useAuthStore((state) => state.user)
  const taskQuery = useTask(taskId)
  const managedUsersQuery = useManagedUsers(sessionUser?.id, access.isManager)
  const updateMutation = useUpdateTask(taskId ?? '')
  const typeLabel = useTaskTypeLabel()
  const [errorMessage, setErrorMessage] = useState('')

  const task = taskQuery.data

  const defaultValues = useMemo<TaskFormValues | undefined>(() => {
    if (!task) return undefined
    return {
      title: task.title ?? '',
      description: task.description ?? '',
      status: statusFromName(task.statusName) ?? 'Pending',
      startTime: apiDateToInput(task.startTime),
      endTime: apiDateToInput(task.endTime),
      assignedToUserId: task.assignedToUser?.id ?? '',
    }
  }, [task])

  // الباك يسمح بالإسناد: لنفسك، أو لأي أحد إن كنت مدير الإدارة، أو لموظفي أقسامك إن كنت مديراً.
  // القائمة = المنفذ الحالي + أنت + موظفو أقسامك (من /lookups/managers/{me}/users)، بلا تكرار.
  const assigneeOptions = useMemo<SelectOption[]>(() => {
    // المفتاح بأحرف صغيرة لأن الـ Guid قد يصل بحالات أحرف مختلفة من مصادر مختلفة
    const options = new Map<string, SelectOption>()
    const add = (id: string | undefined, name: string | undefined) => {
      if (id && !options.has(id.toLowerCase())) {
        options.set(id.toLowerCase(), { value: id, label: name || '—' })
      }
    }
    add(task?.assignedToUser?.id, task?.assignedToUser?.name)
    add(sessionUser?.id, sessionUser?.fullName)
    for (const user of managedUsersQuery.data ?? []) add(String(user.id), user.name)
    return [...options.values()]
  }, [task, sessionUser, managedUsersQuery.data])

  const header = <PageHeader breadcrumb={t('tasks.editBreadcrumb')} title={t('tasks.editTitle')} />

  if (taskQuery.isLoading || (access.isManager && managedUsersQuery.isLoading)) {
    return (
      <section className="space-y-5">
        {header}
        <div
          className="space-y-3 rounded-2xl border border-slate-200 bg-white p-6"
          aria-busy="true"
        >
          {Array.from({ length: 6 }, (_, index) => (
            <div key={index} className="h-10 animate-pulse rounded-lg bg-slate-100" />
          ))}
        </div>
      </section>
    )
  }

  if (!taskId || taskQuery.isError || !task || !defaultValues) {
    return (
      <section className="space-y-5">
        {header}
        <div className="rounded-2xl border border-slate-200 bg-white">
          <ErrorState
            error={taskQuery.error ?? { status: 404, message: t('tasks.notFound') }}
            onRetry={taskId ? () => void taskQuery.refetch() : undefined}
          />
        </div>
        <BackLink to={paths.tasks} label={t('tasks.backToList')} />
      </section>
    )
  }

  if (!canEditTask(task, access, sessionUser?.id, managedUsersQuery.data)) {
    return (
      <section className="space-y-5">
        {header}
        <div className="flex items-start gap-3 rounded-2xl border border-amber-200 bg-amber-50 p-6 text-amber-900">
          <ShieldAlert aria-hidden="true" className="mt-0.5 size-5 shrink-0" />
          <div>
            <h2 className="font-semibold">{t('tasks.editForbiddenTitle')}</h2>
            <p className="mt-1 text-sm leading-7">{t('tasks.editForbidden')}</p>
          </div>
        </div>
        <BackLink to={paths.task(task.id)} label={t('tasks.backToTask')} />
      </section>
    )
  }

  const submit = (values: TaskFormValues) => {
    setErrorMessage('')
    updateMutation.mutate(
      {
        title: values.title.trim(),
        description: values.description.trim() || undefined,
        status: values.status,
        startTime: inputToApiDate(values.startTime),
        endTime: inputToApiDate(values.endTime),
        assignedToUserId: values.assignedToUserId,
      },
      {
        onSuccess: () => {
          toast.success(t('tasks.updateSuccess'))
          navigate(paths.task(task.id))
        },
        onError: (error) => {
          setErrorMessage(describeApiError(error, t))
          toast.error(t('tasks.updateFailed'))
        },
      },
    )
  }

  return (
    <section className="space-y-5">
      <PageHeader
        breadcrumb={t('tasks.editBreadcrumb')}
        title={t('tasks.editTitle')}
        description={task.title}
      />

      <div className="grid items-start gap-5 lg:grid-cols-[1.6fr_1fr]">
        <TaskForm
          defaultValues={defaultValues}
          assigneeOptions={assigneeOptions}
          isPending={updateMutation.isPending}
          errorMessage={errorMessage || undefined}
          onSubmit={submit}
          onCancel={() => navigate(paths.task(task.id))}
        />

        <aside className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
          <h2 className="flex items-center gap-2 font-semibold text-slate-900">
            <Lock aria-hidden="true" className="size-4 text-slate-400" />
            {t('tasks.readOnlyCard')}
          </h2>
          <p className="mt-1 text-xs leading-6 text-slate-500">{t('tasks.readOnlyHint')}</p>
          <dl className="mt-4 space-y-4">
            <ReadOnlyItem label={t('tasks.typeLabel')}>{typeLabel(task.typeName)}</ReadOnlyItem>
            <ReadOnlyItem label={t('tasks.parentTask')}>
              {task.parentTask ? (
                <Link
                  to={paths.task(task.parentTask.id)}
                  className="text-primary hover:underline"
                  dir="auto"
                >
                  {task.parentTask.title}
                </Link>
              ) : (
                t('tasks.noParent')
              )}
            </ReadOnlyItem>
            <ReadOnlyItem label={t('tasks.createdBy')}>
              <span dir="auto">{task.createdByUser?.name || '—'}</span>
            </ReadOnlyItem>
          </dl>
        </aside>
      </div>

      <BackLink to={paths.task(task.id)} label={t('tasks.backToTask')} />
    </section>
  )
}

function ReadOnlyItem({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="space-y-1">
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd className="text-sm font-medium text-slate-800">{children}</dd>
    </div>
  )
}

function BackLink({ to, label }: { to: string; label: string }) {
  return (
    <Link to={to} className="inline-flex text-sm font-medium text-primary hover:underline">
      {label}
    </Link>
  )
}
