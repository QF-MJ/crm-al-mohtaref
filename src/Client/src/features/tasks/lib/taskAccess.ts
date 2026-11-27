import type { LookupItem } from '@/features/lookups'
import type { Access } from '@/permissions/access'
import type { TaskDetail } from '../types'

const sameId = (a: string | undefined, b: string | undefined) =>
  Boolean(a && b && a.toLowerCase() === b.toLowerCase())

/**
 * نفس قاعدة UpdateTaskCommandHandler.IsOwnerAsync في الباك:
 * منشئ المهمة، أو مدير الإدارة، أو مدير في أحد أقسام المنفذ.
 * "مدير في أحد أقسام المنفذ" = المنفذ موجود ضمن /lookups/managers/{me}/users.
 * الواجهة تُخفي الأزرار فقط؛ الباك هو من يفرض القاعدة فعلياً.
 */
export function canEditTask(
  task: TaskDetail,
  access: Access,
  currentUserId: string | undefined,
  managedUsers: LookupItem[] | undefined,
): boolean {
  if (access.isAdmin) return true
  if (sameId(task.createdByUser.id, currentUserId)) return true
  if (!access.isManager || !task.assignedToUser) return false
  // القائمة تستثني المدير نفسه، لكنه مدير في قسمه فيحق له تعديل مهمة مسندة إليه
  if (sameId(task.assignedToUser.id, currentUserId)) return true
  return Boolean(managedUsers?.some((user) => sameId(user.id, task.assignedToUser?.id)))
}
