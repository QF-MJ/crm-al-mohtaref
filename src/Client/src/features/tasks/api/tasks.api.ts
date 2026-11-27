import { api } from '@/api/client'
import type { TaskDetail, TaskFilters, TaskId, TasksPage, TaskUpdateInput } from '../types'

// معاملات الاستعلام بـ PascalCase كما في GetTasksRequest (ASP.NET لا يفرّق بين الحالتين أصلاً)
export const getTasks = (filters: TaskFilters, signal?: AbortSignal) =>
  api
    .get<TasksPage>('/tasks', {
      params: {
        PageNumber: filters.pageNumber,
        Title: filters.title,
        Type: filters.type,
        StartTime: filters.startTime,
        EndTime: filters.endTime,
        AssignedToUserId: filters.assignedToUserId,
        GroupBySubTask: filters.groupBySubTask,
      },
      signal,
    })
    .then((response) => response.data)

export const getTask = (id: TaskId, signal?: AbortSignal) =>
  api
    .get<TaskDetail>(`/tasks/${encodeURIComponent(id)}`, { signal })
    .then((response) => response.data)

// الباك يستبدل كل الحقول (PUT) ويرجّع 204
export const updateTask = (id: TaskId, body: TaskUpdateInput) =>
  api.put<void>(`/tasks/${encodeURIComponent(id)}`, body).then(() => undefined)
