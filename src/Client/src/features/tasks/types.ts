import type { PageInfo } from '@/types/common'

// أشكال الرد مطابقة لـ DTOs في الباك (feature/task/update) وليس للـ yaml عند الاختلاف.
// القيم الفارغة لا تُرسل أصلاً (JsonIgnore WhenWritingNull)، لذلك الحقول الاختيارية بـ ?.

/** معرّف المهمة Guid */
export type TaskId = string

/** قيمة تعداد TaskStatus في الباك (تُرسل بالاسم عبر JsonStringEnumConverter) */
export type TaskStatusValue = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled' | 'OnHold'

/** قيمة تعداد TaskType في الباك */
export type TaskTypeValue =
  | 'Normal'
  | 'Issue'
  | 'Campaign'
  | 'Contact'
  | 'FollowUp'
  | 'Confirmation'
  | 'Placement'
  | 'SubTask'

/** TaskDto: عنصر في القائمة المسطّحة */
export interface TaskListItem {
  id: TaskId
  title: string
  /** الاسم العربي كما يرسله الباك (مثلاً "متابعة") */
  typeName: string
  /** الاسم العربي كما يرسله الباك (مثلاً "قيد التنفيذ") */
  statusName: string
  endTime: string
  assignedToUserFullName?: string
}

/** GroupContactDto */
export interface GroupContact {
  id: string
  fullName: string
  phoneNumber: string
}

/** GroupSubTaskDto */
export interface GroupSubTask {
  id: TaskId
  statusName: string
  contacts: GroupContact[]
}

/** TaskGroupDto: المهمة الأم وعنوانها ومهامها الفرعية */
export interface TaskGroup {
  title: string
  subTasks: GroupSubTask[]
}

/**
 * TasksPageDto. في الباك: groupBySubTask=false يرجّع tasks فقط،
 * و groupBySubTask=true يرجّع groupTasks فقط (المفتاح = معرّف المهمة الأم).
 */
export interface TasksPage {
  page?: PageInfo
  tasks?: TaskListItem[]
  groupTasks?: Record<TaskId, TaskGroup>
}

export interface TaskFilters {
  pageNumber: number
  title?: string
  type?: TaskTypeValue
  /** ISO — إلزامي في الباك */
  startTime: string
  /** ISO — إلزامي في الباك */
  endTime: string
  /** Guid — إلزامي في الباك (الواجهة ترسل المستخدم الحالي افتراضياً) */
  assignedToUserId: string
  groupBySubTask: boolean
}

/** TaskUserDto */
export interface TaskUser {
  id: string
  name: string
}

/** ParentTaskDto */
export interface ParentTask {
  id: TaskId
  title: string
  typeName: string
}

/** LeadStatusDto: حالة الطالب في الحملة */
export interface TaskLeadStatus {
  id: number
  name: string
}

/** TaskStudentDto */
export interface TaskStudent {
  studentId: string
  studentName: string
  leadStatus?: TaskLeadStatus
}

/** TaskContactDto: جهة التواصل (ولي الأمر) وطلابها */
export interface TaskContact extends GroupContact {
  students?: TaskStudent[]
}

/** TaskSubTaskDto */
export interface TaskSubTask {
  id: TaskId
  statusName: string
  contacts: TaskContact[]
}

/** TaskDetailsDto */
export interface TaskDetail {
  id: TaskId
  title: string
  description?: string
  typeName: string
  statusName: string
  startTime: string
  endTime: string
  createdByUser: TaskUser
  assignedToUser?: TaskUser
  parentTask?: ParentTask
  subTasks?: TaskSubTask[]
  contacts?: TaskContact[]
}

/** UpdateTaskRequest كما في الكود (وليس statusName كما في الـ yaml) */
export interface TaskUpdateInput {
  title: string
  description?: string
  status: TaskStatusValue
  startTime: string
  endTime: string
  assignedToUserId: string
}
