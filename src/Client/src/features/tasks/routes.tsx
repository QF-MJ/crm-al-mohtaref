import type { AppRoute } from '@/types/router'
import { TaskDetailPage } from './pages/TaskDetailPage'
import { TaskEditPage } from './pages/TaskEditPage'
import { TasksPage } from './pages/TasksPage'

// صلاحية التعديل تُحسم لكل مهمة على حدة (المنشئ/مدير الإدارة/مدير قسم المنفذ)،
// لذلك صفحة التعديل تفحصها بنفسها، والباك يرفض بـ 403 في كل الأحوال.
export const tasksRoutes: AppRoute[] = [
  {
    path: 'tasks',
    element: <TasksPage />,
    handle: { capability: 'tasks.view' },
  },
  {
    path: 'tasks/:taskId',
    element: <TaskDetailPage />,
    handle: { capability: 'tasks.view' },
  },
  {
    path: 'tasks/:taskId/edit',
    element: <TaskEditPage />,
    handle: { capability: 'tasks.view' },
  },
]
