import { useMemo, useState, type FormEvent, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ListChecks, Phone, Search } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Select, type SelectOption } from '@/components/ui/select'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { TableSkeleton } from '@/components/common/TableSkeleton'
import { Pagination } from '@/components/common/Pagination'
import { paths } from '@/app/router/paths'
import { useAccess } from '@/permissions/useAccess'
import { useAuthStore } from '@/stores/auth.store'
import { useManagedUsers } from '@/features/lookups'
import { cn } from '@/lib/utils'
import { useTasks } from '../api/tasks.queries'
import { FILTERABLE_TASK_TYPES } from '../taskEnums'
import { TaskStatusBadge } from '../components/TaskLabels'
import { useTaskTypeLabel } from '../hooks/useTaskTypeLabel'
import { addMonths, dayEnd, dayStart, formatDateTime, toDateInput } from '../lib/dates'
import type { TaskFilters, TaskGroup, TaskListItem, TaskTypeValue } from '../types'

interface DraftFilters {
  title: string
  type: string
  from: string
  to: string
  assignedToUserId: string
}

// الباك يشترط فترة زمنية: افتراضياً من سنة مضت حتى ثلاثة أشهر قادمة
function defaultRange() {
  const today = new Date()
  return { from: toDateInput(addMonths(today, -12)), to: toDateInput(addMonths(today, 3)) }
}

function toFilters(draft: DraftFilters, groupBySubTask: boolean, userId: string): TaskFilters {
  return {
    pageNumber: 1,
    title: draft.title.trim() || undefined,
    type: (draft.type || undefined) as TaskTypeValue | undefined,
    startTime: dayStart(draft.from),
    endTime: dayEnd(draft.to),
    assignedToUserId: draft.assignedToUserId || userId,
    groupBySubTask,
  }
}

export function TasksPage() {
  const { t, i18n } = useTranslation()
  const access = useAccess()
  const userId = useAuthStore((state) => state.user?.id) ?? ''
  const typeLabel = useTaskTypeLabel()

  const emptyDraft = useMemo<DraftFilters>(
    () => ({ title: '', type: '', ...defaultRange(), assignedToUserId: '' }),
    [],
  )
  const [draft, setDraft] = useState<DraftFilters>(emptyDraft)
  const [filters, setFilters] = useState<TaskFilters>(() => toFilters(emptyDraft, false, userId))

  // المدير يتابع مهام موظفي أقسامه؛ الموظف يرى مهامه فقط فلا نطلب القائمة أصلاً
  const managedUsersQuery = useManagedUsers(userId, access.isManager)
  const assigneeOptions: SelectOption[] = useMemo(() => {
    const others = (managedUsersQuery.data ?? []).filter(
      (user) => user.id.toLowerCase() !== userId.toLowerCase(),
    )
    return [
      { value: '', label: t('tasks.myTasks') },
      ...others.map((user) => ({ value: String(user.id), label: user.name })),
    ]
  }, [managedUsersQuery.data, userId, t])

  const tasksQuery = useTasks(filters, Boolean(userId))

  const rangeInvalid = !draft.from || !draft.to || draft.from > draft.to

  const applyFilters = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (rangeInvalid) return
    setFilters(toFilters(draft, filters.groupBySubTask, userId))
  }

  const clearFilters = () => {
    setDraft(emptyDraft)
    setFilters(toFilters(emptyDraft, filters.groupBySubTask, userId))
  }

  const setGrouped = (groupBySubTask: boolean) =>
    setFilters((current) => ({ ...current, groupBySubTask, pageNumber: 1 }))

  const typeOptions: SelectOption[] = [
    { value: '', label: t('tasks.allTypes') },
    ...FILTERABLE_TASK_TYPES.map((type) => ({
      value: type.value,
      label: t(`tasks.type.${type.value}`),
    })),
  ]

  const hasFilters = Boolean(filters.title || filters.type)
  const data = tasksQuery.data
  const tasks = data?.tasks ?? []
  const groups = Object.entries(data?.groupTasks ?? {})
  const isEmpty = filters.groupBySubTask ? groups.length === 0 : tasks.length === 0
  const pageNumber = filters.pageNumber
  const viewingOther = Boolean(filters.assignedToUserId && filters.assignedToUserId !== userId)

  const goToPage = (next: number) =>
    setFilters((current) => ({ ...current, pageNumber: Math.max(1, next) }))

  return (
    <section className="space-y-5">
      <PageHeader
        breadcrumb={t('tasks.breadcrumb')}
        title={t('tasks.title')}
        description={access.isManager ? t('tasks.subtitleManager') : t('tasks.subtitle')}
      />

      <form
        onSubmit={applyFilters}
        aria-label={t('tasks.filters')}
        className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"
      >
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          <FilterField label={t('tasks.taskTitle')}>
            <Input
              value={draft.title}
              maxLength={200}
              placeholder={t('tasks.titlePlaceholder')}
              onChange={(event) =>
                setDraft((current) => ({ ...current, title: event.target.value }))
              }
            />
          </FilterField>
          <FilterField label={t('tasks.typeLabel')}>
            <Select
              value={draft.type}
              onValueChange={(type) => setDraft((current) => ({ ...current, type }))}
              options={typeOptions}
            />
          </FilterField>
          {access.isManager && (
            <FilterField label={t('tasks.assignee')}>
              <Select
                value={draft.assignedToUserId}
                onValueChange={(assignedToUserId) =>
                  setDraft((current) => ({ ...current, assignedToUserId }))
                }
                options={assigneeOptions}
                disabled={managedUsersQuery.isLoading}
              />
            </FilterField>
          )}
          <FilterField label={t('tasks.from')}>
            <Input
              type="date"
              required
              value={draft.from}
              max={draft.to || undefined}
              onChange={(event) =>
                setDraft((current) => ({ ...current, from: event.target.value }))
              }
            />
          </FilterField>
          <FilterField label={t('tasks.to')}>
            <Input
              type="date"
              required
              value={draft.to}
              min={draft.from || undefined}
              onChange={(event) => setDraft((current) => ({ ...current, to: event.target.value }))}
            />
          </FilterField>
        </div>

        <p className="mt-3 text-xs leading-6 text-slate-500">{t('tasks.rangeHint')}</p>
        {rangeInvalid && (
          <p role="alert" className="mt-1 text-sm text-destructive">
            {t('tasks.rangeInvalid')}
          </p>
        )}
        {managedUsersQuery.isError && (
          <p role="alert" className="mt-1 text-sm text-destructive">
            {t('tasks.assigneesError')}
          </p>
        )}

        <div className="mt-4 flex flex-wrap justify-center gap-2">
          <Button type="submit" size="sm" disabled={rangeInvalid}>
            <Search aria-hidden="true" />
            {t('actions.search')}
          </Button>
          <Button type="button" variant="outline" size="sm" onClick={clearFilters}>
            {t('tasks.clearFilters')}
          </Button>
        </div>
      </form>

      <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
        <header className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 px-4 py-4 sm:px-5">
          <div>
            <h2 className="font-semibold text-slate-900">
              {filters.groupBySubTask ? t('tasks.groupedTitle') : t('tasks.listTitle')}
            </h2>
            <p className="mt-1 text-xs leading-6 text-slate-500">
              {filters.groupBySubTask ? t('tasks.groupedDescription') : t('tasks.listDescription')}
            </p>
          </div>
          <div
            role="group"
            aria-label={t('tasks.viewMode')}
            className="inline-flex rounded-lg border border-slate-200 bg-slate-50 p-0.5"
          >
            <ViewToggle active={!filters.groupBySubTask} onClick={() => setGrouped(false)}>
              {t('tasks.viewList')}
            </ViewToggle>
            <ViewToggle active={filters.groupBySubTask} onClick={() => setGrouped(true)}>
              {t('tasks.viewGrouped')}
            </ViewToggle>
          </div>
        </header>

        {tasksQuery.isLoading ? (
          <TableSkeleton rows={6} columns={5} />
        ) : tasksQuery.isError ? (
          <ErrorState
            error={tasksQuery.error}
            onRetry={() => void tasksQuery.refetch()}
            isRetrying={tasksQuery.isFetching}
          />
        ) : isEmpty ? (
          <EmptyState
            icon={ListChecks}
            title={hasFilters ? t('tasks.emptyFiltered') : t('tasks.empty')}
            description={hasFilters ? t('tasks.emptyFilteredHint') : t('tasks.emptyHint')}
            action={
              pageNumber > 1 ? (
                <Button type="button" size="sm" onClick={() => goToPage(pageNumber - 1)}>
                  {t('pagination.previous')}
                </Button>
              ) : null
            }
          />
        ) : (
          <>
            {filters.groupBySubTask ? (
              <ul className="divide-y divide-slate-100">
                {groups.map(([parentId, group]) => (
                  <TaskGroupItem key={parentId} parentId={parentId} group={group} />
                ))}
              </ul>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full min-w-[760px] text-start text-sm">
                  <thead className="bg-slate-50 text-xs font-medium text-slate-500">
                    <tr>
                      <th scope="col" className="px-4 py-3 text-start">
                        {t('tasks.taskTitle')}
                      </th>
                      <th scope="col" className="px-4 py-3 text-start">
                        {t('tasks.typeLabel')}
                      </th>
                      <th scope="col" className="px-4 py-3 text-start">
                        {t('tasks.statusLabel')}
                      </th>
                      <th scope="col" className="px-4 py-3 text-start">
                        {t('tasks.endTime')}
                      </th>
                      {viewingOther && (
                        <th scope="col" className="px-4 py-3 text-start">
                          {t('tasks.assignee')}
                        </th>
                      )}
                      <th scope="col" className="px-4 py-3 text-start">
                        {t('tasks.actions')}
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {tasks.map((task) => (
                      <TaskRow
                        key={task.id}
                        task={task}
                        typeLabel={typeLabel(task.typeName)}
                        endTime={formatDateTime(task.endTime, i18n.language)}
                        showAssignee={viewingOther}
                      />
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            <Pagination
              pageNumber={pageNumber}
              hasNextPage={Boolean(data?.page?.hasNextPage)}
              isFetching={tasksQuery.isFetching}
              onPrevious={() => goToPage(pageNumber - 1)}
              onNext={() => goToPage(pageNumber + 1)}
            />
          </>
        )}
      </section>
    </section>
  )
}

function TaskRow({
  task,
  typeLabel,
  endTime,
  showAssignee,
}: {
  task: TaskListItem
  typeLabel: string
  endTime: string
  showAssignee: boolean
}) {
  const { t } = useTranslation()
  return (
    <tr className="transition-colors hover:bg-teal-50/50">
      <td className="max-w-[320px] px-4 py-3 font-medium text-slate-800">
        <Link
          to={paths.task(task.id)}
          className="block truncate hover:text-primary hover:underline"
          dir="auto"
          title={task.title}
        >
          {task.title?.trim() || t('tasks.untitled')}
        </Link>
      </td>
      <td className="px-4 py-3 text-slate-600">{typeLabel}</td>
      <td className="px-4 py-3">
        <TaskStatusBadge statusName={task.statusName} />
      </td>
      <td className="px-4 py-3 text-slate-600">{endTime}</td>
      {showAssignee && (
        <td className="px-4 py-3 text-slate-600" dir="auto">
          {task.assignedToUserFullName || '—'}
        </td>
      )}
      <td className="px-4 py-3">
        <Link
          to={paths.task(task.id)}
          className="inline-flex h-8 items-center rounded-lg border border-slate-200 bg-white px-3 text-xs font-medium text-slate-700 hover:bg-slate-50"
        >
          {t('actions.view')}
        </Link>
      </td>
    </tr>
  )
}

function TaskGroupItem({ parentId, group }: { parentId: string; group: TaskGroup }) {
  const { t } = useTranslation()
  return (
    <li className="px-4 py-4 sm:px-5">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <Link
          to={paths.task(parentId)}
          className="font-medium text-slate-900 hover:text-primary hover:underline"
          dir="auto"
        >
          {group.title?.trim() || t('tasks.untitled')}
        </Link>
        <Badge variant="neutral">{t('tasks.subTaskCount', { count: group.subTasks.length })}</Badge>
      </div>
      <ul className="mt-3 grid gap-2 lg:grid-cols-2">
        {group.subTasks.map((subTask) => (
          <li
            key={subTask.id}
            className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-slate-200 bg-slate-50/60 px-3 py-2"
          >
            <div className="min-w-0 space-y-1">
              {subTask.contacts.length === 0 ? (
                <p className="text-xs text-slate-500">{t('tasks.noContacts')}</p>
              ) : (
                subTask.contacts.map((contact) => (
                  <p key={contact.id} className="flex flex-wrap items-center gap-2 text-xs">
                    <span className="font-medium text-slate-700" dir="auto">
                      {contact.fullName}
                    </span>
                    <span className="flex items-center gap-1 text-slate-500" dir="ltr">
                      <Phone aria-hidden="true" className="size-3" />
                      {contact.phoneNumber}
                    </span>
                  </p>
                ))
              )}
            </div>
            <div className="flex items-center gap-2">
              <TaskStatusBadge statusName={subTask.statusName} />
              <Link
                to={paths.task(subTask.id)}
                className="inline-flex h-7 items-center rounded-lg border border-slate-200 bg-white px-2.5 text-xs font-medium text-slate-700 hover:bg-slate-50"
              >
                {t('actions.view')}
              </Link>
            </div>
          </li>
        ))}
      </ul>
    </li>
  )
}

function ViewToggle({
  active,
  onClick,
  children,
}: {
  active: boolean
  onClick: () => void
  children: ReactNode
}) {
  return (
    <button
      type="button"
      aria-pressed={active}
      onClick={onClick}
      className={cn(
        'h-8 rounded-md px-3 text-xs font-medium transition-colors',
        active ? 'bg-white text-teal-800 shadow-sm' : 'text-slate-500 hover:text-slate-700',
      )}
    >
      {children}
    </button>
  )
}

function FilterField({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="space-y-1.5 text-xs font-medium text-slate-500">
      <span>{label}</span>
      {children}
    </label>
  )
}
