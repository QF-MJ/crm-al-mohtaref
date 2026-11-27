import { useState, type ReactNode } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { ArrowRight, Info, ListTree, Pencil, RefreshCw, Users } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Select } from '@/components/ui/select'
import { PageHeader } from '@/components/common/PageHeader'
import { ErrorState } from '@/components/common/ErrorState'
import { paths } from '@/app/router/paths'
import { describeApiError } from '@/api/errors'
import { useAccess } from '@/permissions/useAccess'
import { useAuthStore } from '@/stores/auth.store'
import { useManagedUsers } from '@/features/lookups'
import { useTask, useUpdateTask } from '../api/tasks.queries'
import { TASK_STATUSES, statusFromName } from '../taskEnums'
import { TaskStatusBadge } from '../components/TaskLabels'
import { useTaskTypeLabel } from '../hooks/useTaskTypeLabel'
import { TaskContactsList } from '../components/TaskContactsList'
import { canEditTask } from '../lib/taskAccess'
import { formatDateTime } from '../lib/dates'
import type { TaskDetail, TaskStatusValue } from '../types'

export function TaskDetailPage() {
  const { t, i18n } = useTranslation()
  const { taskId } = useParams<{ taskId: string }>()
  const access = useAccess()
  const userId = useAuthStore((state) => state.user?.id)
  const taskQuery = useTask(taskId)
  const managedUsersQuery = useManagedUsers(userId, access.isManager)
  const typeLabel = useTaskTypeLabel()

  if (taskQuery.isLoading) {
    return (
      <section className="space-y-5">
        <PageHeader breadcrumb={t('tasks.detailBreadcrumb')} title={t('tasks.detailTitle')} />
        <div
          className="space-y-3 rounded-2xl border border-slate-200 bg-white p-6"
          aria-busy="true"
        >
          {Array.from({ length: 5 }, (_, index) => (
            <div key={index} className="h-10 animate-pulse rounded-lg bg-slate-100" />
          ))}
        </div>
      </section>
    )
  }

  if (!taskId || taskQuery.isError || !taskQuery.data) {
    return (
      <section className="space-y-5">
        <PageHeader breadcrumb={t('tasks.detailBreadcrumb')} title={t('tasks.detailTitle')} />
        <div className="rounded-2xl border border-slate-200 bg-white">
          <ErrorState
            error={taskQuery.error ?? { status: 404, message: t('tasks.notFound') }}
            onRetry={taskId ? () => void taskQuery.refetch() : undefined}
          />
        </div>
        <BackLink />
      </section>
    )
  }

  const task = taskQuery.data
  const canEdit = canEditTask(task, access, userId, managedUsersQuery.data)
  const locale = i18n.language

  return (
    <section className="space-y-5">
      <PageHeader
        breadcrumb={t('tasks.detailBreadcrumb')}
        title={task.title?.trim() || t('tasks.untitled')}
        description={t('tasks.detailSubtitle', {
          type: typeLabel(task.typeName),
          assignee: task.assignedToUser?.name ?? '—',
        })}
        actions={
          canEdit ? (
            <Link
              to={paths.taskEdit(task.id)}
              className="inline-flex h-9 items-center gap-2 rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground hover:bg-primary/90"
            >
              <Pencil aria-hidden="true" className="size-4" />
              {t('tasks.edit')}
            </Link>
          ) : null
        }
      />

      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <dl className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <SummaryItem label={t('tasks.assignee')}>
            <span dir="auto">{task.assignedToUser?.name || '—'}</span>
          </SummaryItem>
          <SummaryItem label={t('tasks.startTime')}>
            {formatDateTime(task.startTime, locale)}
          </SummaryItem>
          <SummaryItem label={t('tasks.endTime')}>
            {formatDateTime(task.endTime, locale)}
          </SummaryItem>
          <SummaryItem label={t('tasks.statusLabel')}>
            <TaskStatusBadge statusName={task.statusName} />
          </SummaryItem>
        </dl>
      </section>

      <div className="grid gap-5 lg:grid-cols-2">
        <Card icon={Info} title={t('tasks.detailsCard')} description={t('tasks.detailsCardHint')}>
          <div className="space-y-4">
            <div>
              <p className="text-xs text-slate-500">{t('tasks.description')}</p>
              <p
                className="mt-1 whitespace-pre-line rounded-xl bg-slate-50 p-3 text-sm leading-7 text-slate-700"
                dir="auto"
              >
                {task.description?.trim() || t('tasks.noDescription')}
              </p>
            </div>
            <dl className="grid gap-4 sm:grid-cols-2">
              <SummaryItem label={t('tasks.typeLabel')}>{typeLabel(task.typeName)}</SummaryItem>
              <SummaryItem label={t('tasks.createdBy')}>
                <span dir="auto">{task.createdByUser?.name || '—'}</span>
              </SummaryItem>
              {task.parentTask && (
                <SummaryItem label={t('tasks.parentTask')}>
                  <Link
                    to={paths.task(task.parentTask.id)}
                    className="text-primary hover:underline"
                    dir="auto"
                  >
                    {task.parentTask.title}
                  </Link>
                  <span className="ms-2 text-xs text-slate-500">
                    ({typeLabel(task.parentTask.typeName)})
                  </span>
                </SummaryItem>
              )}
            </dl>
          </div>
        </Card>

        {task.contacts && (
          <Card
            icon={Users}
            title={t('tasks.contactsCard')}
            description={t('tasks.contactsCardHint')}
          >
            <TaskContactsList contacts={task.contacts} />
          </Card>
        )}

        {task.subTasks && (
          <Card
            icon={ListTree}
            title={t('tasks.subTasksCard')}
            description={t('tasks.subTaskCount', { count: task.subTasks.length })}
          >
            {task.subTasks.length === 0 ? (
              <p className="text-sm text-slate-500">{t('tasks.noSubTasks')}</p>
            ) : (
              <ul className="space-y-3">
                {task.subTasks.map((subTask, index) => (
                  <li key={subTask.id} className="rounded-xl border border-slate-200 p-3">
                    <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
                      <Link
                        to={paths.task(subTask.id)}
                        className="text-sm font-medium text-primary hover:underline"
                      >
                        {t('tasks.subTaskNumber', { number: index + 1 })}
                      </Link>
                      <TaskStatusBadge statusName={subTask.statusName} />
                    </div>
                    <TaskContactsList contacts={subTask.contacts} />
                  </li>
                ))}
              </ul>
            )}
          </Card>
        )}

        <StatusCard task={task} canEdit={canEdit} />
      </div>

      <BackLink />
    </section>
  )
}

// تغيير الحالة (TM-02) عبر PUT نفسه: نرسل باقي الحقول كما هي ونغيّر الحالة فقط.
// الباك يسمح بالتعديل للمنشئ ومدير الإدارة ومدير قسم المنفذ فقط، لا للمنفذ نفسه.
function StatusCard({ task, canEdit }: { task: TaskDetail; canEdit: boolean }) {
  const { t } = useTranslation()
  const currentStatus = statusFromName(task.statusName)
  const [status, setStatus] = useState<TaskStatusValue | ''>(currentStatus ?? '')
  const updateMutation = useUpdateTask(task.id)

  const statusOptions = TASK_STATUSES.map((item) => ({
    value: item.value,
    label: t(`tasks.status.${item.value}`),
  }))

  const save = () => {
    if (!status || !task.assignedToUser) return
    updateMutation.mutate(
      {
        title: task.title,
        description: task.description?.trim() || undefined,
        status,
        startTime: task.startTime,
        endTime: task.endTime,
        assignedToUserId: task.assignedToUser.id,
      },
      {
        onSuccess: () => toast.success(t('tasks.statusUpdated')),
        onError: (error) => toast.error(describeApiError(error, t)),
      },
    )
  }

  return (
    <Card icon={RefreshCw} title={t('tasks.statusCard')} description={t('tasks.statusCardHint')}>
      <div className="space-y-4">
        <div className="flex items-center justify-between gap-2 text-sm">
          <span className="text-slate-500">{t('tasks.currentStatus')}</span>
          <TaskStatusBadge statusName={task.statusName} />
        </div>
        {canEdit ? (
          <>
            <label className="block space-y-1.5 text-xs font-medium text-slate-500">
              <span>{t('tasks.newStatus')}</span>
              <Select
                value={status}
                onValueChange={(value) => setStatus(value as TaskStatusValue)}
                options={statusOptions}
              />
            </label>
            <Button
              type="button"
              size="sm"
              onClick={save}
              disabled={!status || status === currentStatus || updateMutation.isPending}
            >
              {updateMutation.isPending ? t('tasks.saving') : t('tasks.saveStatus')}
            </Button>
          </>
        ) : (
          <p className="rounded-xl bg-amber-50 px-3 py-2 text-xs leading-6 text-amber-800">
            {t('tasks.statusReadOnly')}
          </p>
        )}
      </div>
    </Card>
  )
}

function Card({
  icon: Icon,
  title,
  description,
  children,
}: {
  icon: typeof Info
  title: string
  description?: string
  children: ReactNode
}) {
  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
      <h2 className="flex items-center gap-2 font-semibold text-slate-900">
        <Icon aria-hidden="true" className="size-4 text-slate-400" />
        {title}
      </h2>
      {description && <p className="mt-1 text-xs leading-6 text-slate-500">{description}</p>}
      <div className="mt-4">{children}</div>
    </section>
  )
}

function SummaryItem({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="space-y-1">
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd className="text-sm font-medium text-slate-800">{children}</dd>
    </div>
  )
}

function BackLink() {
  const { t } = useTranslation()
  return (
    <Link
      to={paths.tasks}
      className="inline-flex items-center gap-2 text-sm font-medium text-primary hover:underline"
    >
      <ArrowRight aria-hidden="true" className="size-4 rtl:rotate-0 ltr:rotate-180" />
      {t('tasks.backToList')}
    </Link>
  )
}
