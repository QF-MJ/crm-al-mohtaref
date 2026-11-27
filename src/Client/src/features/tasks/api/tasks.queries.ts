import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getTask, getTasks, updateTask } from './tasks.api'
import type { TaskFilters, TaskId, TaskUpdateInput } from '../types'

const tasksKey = ['tasks'] as const

export function useTasks(filters: TaskFilters, enabled = true) {
  return useQuery({
    queryKey: [...tasksKey, 'list', filters],
    queryFn: ({ signal }) => getTasks(filters, signal),
    enabled,
  })
}

export function useTask(id: TaskId | undefined) {
  return useQuery({
    queryKey: [...tasksKey, 'detail', id],
    queryFn: ({ signal }) => {
      if (id === undefined) throw new Error('A task ID is required')
      return getTask(id, signal)
    },
    enabled: Boolean(id),
  })
}

export function useUpdateTask(id: TaskId) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: TaskUpdateInput) => updateTask(id, body),
    // التعديل قد يغيّر القائمة (الحالة/الموعد/المنفذ) والتفاصيل، وتفاصيل المهمة الأم
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tasksKey }),
  })
}
