import { http, HttpResponse, delay } from 'msw'
import { env } from '@/lib/env'
import type { TaskDetail, TaskListItem, TasksPage } from '@/features/tasks'
import { mockAccounts, mockSessionAccount, type MockAccount } from './auth.handlers'

// محاكاة مطابقة لسلوك الباك (feature/task/update):
// - GET /tasks: StartTime و EndTime و AssignedToUserId إلزامية، والمهام الفرعية تظهر فقط مع GroupBySubTask=true.
// - GET /tasks/{id}: شكل الرد حسب نوع المهمة (TaskRepository.GetDetailAsync).
// - PUT /tasks/{id}: المنشئ أو مدير الإدارة أو مدير قسم المنفذ فقط.
// - GET /lookups/managers/{id}/users: للمستخدم الحالي فقط.
// البيانات في الذاكرة وتعود لحالتها الأولى عند إعادة تحميل الصفحة.

const PAGE_SIZE = 20
const ADMIN_DEPARTMENT = 'الإدارة'
const MANAGER = 'مدير'

type Status = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled' | 'OnHold'
type Type =
  | 'Normal'
  | 'Issue'
  | 'Campaign'
  | 'Contact'
  | 'FollowUp'
  | 'Confirmation'
  | 'Placement'
  | 'SubTask'

const STATUS_NAMES: Record<Status, string> = {
  Pending: 'قيد الانتظار',
  InProgress: 'قيد التنفيذ',
  Completed: 'مكتملة',
  Cancelled: 'ملغاة',
  OnHold: 'معلقة',
}

const TYPE_NAMES: Record<Type, string> = {
  Normal: 'مهمة عادية',
  Issue: 'مشكلة',
  Campaign: 'حملة',
  Contact: 'تواصل',
  FollowUp: 'متابعة',
  Confirmation: 'تثبيت',
  Placement: 'تسكين',
  SubTask: 'مهمة فرعية',
}

const LEAD_STATUSES = ['جديد', 'جاري التواصل', 'فشل التواصل', 'تم الرد', 'جاري المتابعة']

interface MockStudent {
  id: string
  name: string
  leadStatusId: number
}

interface MockContact {
  id: string
  fullName: string
  phoneNumber: string
  students: MockStudent[]
}

interface MockTask {
  id: string
  title: string
  description?: string
  type: Type
  status: Status
  startTime: string
  endTime: string
  parentId?: string
  createdById: string
  assignedToId: string
  contactIds: string[]
}

const guid = (prefix: number, n: number) =>
  `${String(prefix).padStart(8, '0')}-0000-4000-8000-${String(n).padStart(12, '0')}`

const pad = (value: number) => String(value).padStart(2, '0')

/** تاريخ نسبي لليوم بصيغة الباك (بلا منطقة زمنية) */
function relative(days: number, hour = 9): string {
  const date = new Date()
  date.setDate(date.getDate() + days)
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(hour)}:00:00`
}

const contacts: MockContact[] = [
  ['هاجر طارق', '0933206576', ['ليان طارق', 'عمر طارق']],
  ['صلاح حسن', '0933068665', ['يوسف صلاح']],
  ['نورة العتيبي', '0944123987', ['سارة العتيبي']],
  ['خالد العلي', '0955321654', []],
  ['منى الأحمد', '0966987123', ['ريم الأحمد', 'آدم الأحمد']],
].map(([fullName, phoneNumber, students], index) => ({
  id: guid(7001, index + 1),
  fullName: fullName as string,
  phoneNumber: phoneNumber as string,
  students: (students as string[]).map((name, studentIndex) => ({
    id: guid(7002, index * 10 + studentIndex + 1),
    name,
    leadStatusId: ((index + studentIndex) % LEAD_STATUSES.length) + 1,
  })),
}))

function buildTasks(): MockTask[] {
  const tasks: MockTask[] = []
  let counter = 0
  const add = (task: Omit<MockTask, 'id' | 'contactIds'> & { contactIds?: string[] }) => {
    counter += 1
    const created = { ...task, id: guid(8000, counter), contactIds: task.contactIds ?? [] }
    tasks.push(created)
    return created
  }

  const accounts = mockAccounts()
  const admin = accounts[0]

  for (const account of accounts) {
    const me = account.userId
    add({
      title: 'تجهيز تقرير أسبوعي للإدارة',
      description: 'إعداد ملخص أسبوعي بالمهام المنجزة والعالقة.',
      type: 'Normal',
      status: 'InProgress',
      startTime: relative(-2),
      endTime: relative(3, 17),
      createdById: admin.userId,
      assignedToId: me,
    })
    add({
      title: 'مراجعة ملفات العملاء المؤرشفة',
      type: 'Normal',
      status: 'Completed',
      startTime: relative(-40),
      endTime: relative(-35, 15),
      createdById: me,
      assignedToId: me,
    })

    const issue = add({
      title: 'مشكلة في دخول الطالب للمنصة',
      description: 'متابعة العملاء الذين واجهوا خطأ في تسجيل الدخول.',
      type: 'Issue',
      status: 'Pending',
      startTime: relative(-1),
      endTime: relative(6, 18),
      createdById: admin.userId,
      assignedToId: me,
    })
    for (const contactId of [contacts[3].id, contacts[0].id]) {
      add({
        title: 'اتصال بالعميل لحل مشكلة الدخول',
        type: 'SubTask',
        status: 'Pending',
        startTime: relative(0),
        endTime: relative(2, 12),
        parentId: issue.id,
        createdById: admin.userId,
        assignedToId: me,
        contactIds: [contactId],
      })
    }

    const campaign = add({
      title: 'حملة التسجيل الشتوية 2026',
      description: 'إدارة ومتابعة اتصالات الحملة مع أولياء الأمور.',
      type: 'Campaign',
      status: 'InProgress',
      startTime: relative(-10),
      endTime: relative(20, 18),
      createdById: admin.userId,
      assignedToId: me,
    })
    add({
      title: 'التواصل مع أولياء الأمور - دفعة 1',
      type: 'SubTask',
      status: 'Completed',
      startTime: relative(-9),
      endTime: relative(-5, 16),
      parentId: campaign.id,
      createdById: admin.userId,
      assignedToId: me,
      contactIds: [contacts[0].id, contacts[1].id],
    })

    const followUp = add({
      title: 'متابعة بيانات العملاء المرتبطين بالحملة',
      description:
        'مراجعة بيانات جهات التواصل المرتبطة بالحملة وتسجيل أي حاجة للمتابعة في سجل العميل.',
      type: 'FollowUp',
      status: 'OnHold',
      startTime: relative(-3),
      endTime: relative(4, 17),
      parentId: campaign.id,
      createdById: admin.userId,
      assignedToId: me,
    })
    add({
      title: 'متابعة نورة العتيبي',
      type: 'SubTask',
      status: 'InProgress',
      startTime: relative(-2),
      endTime: relative(1, 13),
      parentId: followUp.id,
      createdById: admin.userId,
      assignedToId: me,
      contactIds: [contacts[2].id, contacts[4].id],
    })
  }

  // مهام إضافية لمدير المبيعات لتجربة التنقل بين الصفحات
  const sales = accounts[1]
  for (let index = 1; index <= 22; index += 1) {
    add({
      title: `تواصل أولي مع عميل محتمل (${index})`,
      type: 'Normal',
      status: index % 3 === 0 ? 'Completed' : 'Pending',
      startTime: relative(-20 + index),
      endTime: relative(-18 + index, 16),
      createdById: sales.userId,
      assignedToId: sales.userId,
    })
  }

  return tasks
}

let tasks = buildTasks()

const error = (status: number, message: string) =>
  HttpResponse.json({ status, message }, { status })

const sameId = (a?: string | null, b?: string | null) =>
  Boolean(a && b && a.toLowerCase() === b.toLowerCase())

const findAccount = (id?: string | null) =>
  mockAccounts().find((account) => sameId(account.userId, id))

const isAdminManager = (me: MockAccount) =>
  me.permissions.some((p) => p.departmentName === ADMIN_DEPARTMENT && p.roleName === MANAGER)

/** CurrentUserService.IsManagerInAnyDepartment: مدير في أحد أقسام الهدف */
function isManagerOf(me: MockAccount, target: MockAccount, includeAdministration = false) {
  const departments = new Set(target.permissions.map((p) => p.departmentName))
  if (includeAdministration) departments.add(ADMIN_DEPARTMENT)
  return me.permissions.some((p) => p.roleName === MANAGER && departments.has(p.departmentName))
}

/** Get*QueryHandler.IsOwnerAsync: صاحب المهام نفسه، أو مدير في قسمه، أو مدير الإدارة */
const canView = (me: MockAccount, assignee: MockAccount) =>
  sameId(me.userId, assignee.userId) || isManagerOf(me, assignee, true)

/** UpdateTaskCommandHandler.IsOwnerAsync */
function canUpdate(me: MockAccount, task: MockTask) {
  if (sameId(task.createdById, me.userId) || isAdminManager(me)) return true
  const assignee = findAccount(task.assignedToId)
  return Boolean(assignee && isManagerOf(me, assignee))
}

const parseDate = (value: string | null) => {
  if (!value) return undefined
  const time = new Date(value.replace(/Z$/, '')).getTime()
  return Number.isNaN(time) ? undefined : time
}

const contactsOf = (task: MockTask, withStudents: boolean) =>
  task.contactIds
    .map((id) => contacts.find((contact) => contact.id === id))
    .filter((contact): contact is MockContact => Boolean(contact))
    .map((contact) => ({
      id: contact.id,
      fullName: contact.fullName,
      phoneNumber: contact.phoneNumber,
      ...(withStudents
        ? {
            students: contact.students.map((student) => ({
              studentId: student.id,
              studentName: student.name,
              leadStatus: {
                id: student.leadStatusId,
                name: LEAD_STATUSES[student.leadStatusId - 1],
              },
            })),
          }
        : {}),
    }))

function toDetail(task: MockTask): TaskDetail {
  const parent = tasks.find((item) => item.id === task.parentId)
  const isIssue = (parent?.type ?? task.type) === 'Issue'
  const subTasks = tasks
    .filter(
      (item) =>
        item.type === 'SubTask' &&
        (item.parentId === task.id ||
          tasks.find((p) => p.id === item.parentId)?.parentId === task.id),
    )
    .sort((a, b) => a.startTime.localeCompare(b.startTime))

  return {
    id: task.id,
    title: task.title,
    ...(task.description ? { description: task.description } : {}),
    typeName: TYPE_NAMES[task.type],
    statusName: STATUS_NAMES[task.status],
    startTime: task.startTime,
    endTime: task.endTime,
    createdByUser: { id: task.createdById, name: findAccount(task.createdById)?.fullName ?? '' },
    assignedToUser: { id: task.assignedToId, name: findAccount(task.assignedToId)?.fullName ?? '' },
    ...(parent
      ? { parentTask: { id: parent.id, title: parent.title, typeName: TYPE_NAMES[parent.type] } }
      : {}),
    ...(task.type !== 'SubTask' && task.type !== 'Normal'
      ? {
          subTasks: subTasks.map((sub) => ({
            id: sub.id,
            statusName: STATUS_NAMES[sub.status],
            contacts: contactsOf(sub, !isIssue),
          })),
        }
      : {}),
    ...(task.type === 'SubTask' ? { contacts: contactsOf(task, !isIssue) } : {}),
  }
}

export const tasksHandlers = [
  http.get(`${env.apiUrl}/tasks`, async ({ request }) => {
    await delay(250)
    const me = mockSessionAccount(request)
    if (!me) return error(401, 'Unauthorized')

    const params = new URL(request.url).searchParams
    const get = (name: string) =>
      params.get(name) ?? params.get(name.charAt(0).toLowerCase() + name.slice(1))
    const pageNumber = Number(get('PageNumber') ?? 1)
    const from = parseDate(get('StartTime'))
    const to = parseDate(get('EndTime'))
    const assigneeId = get('AssignedToUserId')
    const type = get('Type') as Type | null
    const title = get('Title')?.trim()
    const grouped = get('GroupBySubTask') === 'true'

    if (from === undefined) return error(400, 'تاريخ البداية مطلوب')
    if (to === undefined) return error(400, 'تاريخ النهاية مطلوب')
    if (to < from) return error(400, 'تاريخ النهاية يجب أن يكون أكبر من أو يساوي تاريخ البداية')
    if (!assigneeId) return error(400, 'معرف المنفذ مطلوب')
    if (type && (!(type in TYPE_NAMES) || type === 'SubTask')) {
      return error(400, 'لا يمكن الفلترة بنوع مهمة فرعية')
    }

    const assignee = findAccount(assigneeId)
    if (!assignee) return error(404, `المستخدم بالمعرف ${assigneeId} غير موجود `)
    if (!canView(me, assignee)) {
      return error(403, `لا تملك صلاحيات عرض مهام المستخدم بالمعرف ${assigneeId}`)
    }

    const matching = tasks
      .filter(
        (task) =>
          sameId(task.assignedToId, assignee.userId) &&
          (!title || task.title.includes(title)) &&
          (!type || task.type === type) &&
          (parseDate(task.startTime) ?? 0) >= from &&
          (parseDate(task.endTime) ?? 0) <= to,
      )
      .filter((task) =>
        grouped ? task.type === 'SubTask' && task.parentId : task.type !== 'SubTask',
      )
      .sort((a, b) => b.endTime.localeCompare(a.endTime))

    const pageItems = matching.slice((pageNumber - 1) * PAGE_SIZE, pageNumber * PAGE_SIZE)
    const page = { pageNumber, hasNextPage: matching.length > pageNumber * PAGE_SIZE }

    if (grouped) {
      const groupTasks: NonNullable<TasksPage['groupTasks']> = {}
      for (const sub of pageItems) {
        const parent = tasks.find((task) => task.id === sub.parentId)
        if (!parent) continue
        groupTasks[parent.id] ??= { title: parent.title, subTasks: [] }
        groupTasks[parent.id].subTasks.push({
          id: sub.id,
          statusName: STATUS_NAMES[sub.status],
          contacts: contactsOf(sub, false),
        })
      }
      return HttpResponse.json({ page, groupTasks } satisfies TasksPage)
    }

    const items: TaskListItem[] = pageItems.map((task) => ({
      id: task.id,
      title: task.title,
      typeName: TYPE_NAMES[task.type],
      statusName: STATUS_NAMES[task.status],
      endTime: task.endTime,
      assignedToUserFullName: assignee.fullName,
    }))
    return HttpResponse.json({ page, tasks: items } satisfies TasksPage)
  }),

  http.get(`${env.apiUrl}/tasks/:taskId`, async ({ request, params }) => {
    await delay(200)
    const me = mockSessionAccount(request)
    if (!me) return error(401, 'Unauthorized')

    const task = tasks.find((item) => sameId(item.id, String(params.taskId)))
    if (!task) return error(404, `المهمة بالمعرف"${String(params.taskId)}" غير موجودة`)

    const assignee = findAccount(task.assignedToId)
    if (!assignee || !canView(me, assignee)) {
      return error(403, `لا تملك صلاحيات عرض مهام المستخدم بالمعرف ${task.assignedToId}`)
    }
    return HttpResponse.json(toDetail(task))
  }),

  http.put(`${env.apiUrl}/tasks/:taskId`, async ({ request, params }) => {
    await delay(300)
    const me = mockSessionAccount(request)
    if (!me) return error(401, 'Unauthorized')

    const body = (await request.json()) as {
      title?: string
      description?: string | null
      status?: Status
      startTime?: string
      endTime?: string
      assignedToUserId?: string
    }

    // UpdateTaskCommandValidator
    if (!body.title?.trim()) return error(400, 'عنوان المهمة مطلوب')
    if (body.title.length > 200) return error(400, 'يجب ألا يتجاوز عنوان المهمة 200 حرف')
    if (body.description && body.description.length > 2000) {
      return error(400, 'يجب ألا يتجاوز وصف المهمة 2000 حرف')
    }
    if (!body.status || !(body.status in STATUS_NAMES)) return error(400, 'حالة المهمة غير صالحة')
    const start = parseDate(body.startTime ?? null)
    const end = parseDate(body.endTime ?? null)
    if (start === undefined) return error(400, 'وقت بدء المهمة مطلوب')
    if (end === undefined) return error(400, 'وقت انتهاء المهمة مطلوب')
    const startDay = new Date(start)
    startDay.setHours(0, 0, 0, 0)
    if (end < startDay.getTime()) {
      return error(400, 'يجب أن يكون وقت الانتهاء مساوياً لتاريخ البدء أو بعده')
    }
    if (!body.assignedToUserId) return error(400, 'معرّف المستخدم المكلّف مطلوب')

    const task = tasks.find((item) => sameId(item.id, String(params.taskId)))
    if (!task) return error(404, `المهمة بالمعرف ${String(params.taskId)} غير موجودة`)
    if (!canUpdate(me, task)) {
      return error(403, 'تعديل هذه المهمة متاح لمنشئها أو لمدير الإدارة أو لمدير قسم المنفذ فقط')
    }

    if (!sameId(body.assignedToUserId, task.assignedToId)) {
      const assignee = findAccount(body.assignedToUserId)
      if (!assignee) return error(404, `المستخدم بالمعرف ${body.assignedToUserId} غير موجود`)
      const allowed =
        sameId(assignee.userId, me.userId) || isAdminManager(me) || isManagerOf(me, assignee)
      if (!allowed) {
        return error(
          403,
          `لا تملك صلاحية إسناد المهمة إلى المستخدم بالمعرف ${body.assignedToUserId}`,
        )
      }
    }

    tasks = tasks.map((item) =>
      item.id === task.id
        ? {
            ...item,
            title: body.title!.trim(),
            description: body.description?.trim() || undefined,
            status: body.status!,
            startTime: body.startTime!,
            endTime: body.endTime!,
            assignedToId: body.assignedToUserId!,
          }
        : item,
    )
    return new HttpResponse(null, { status: 204 })
  }),

  http.get(`${env.apiUrl}/lookups/managers/:managerId/users`, async ({ request, params }) => {
    await delay(150)
    const me = mockSessionAccount(request)
    if (!me) return error(401, 'Unauthorized')
    if (!sameId(String(params.managerId), me.userId)) {
      return error(403, 'لا يمكنك عرض المستخدمين التابعين لمدير آخر')
    }

    // LookupRepository.GetUsersByManagerIdAsync: مدير الإدارة يرى الجميع،
    // وغيره يرى زملاء أقسامه التي يديرها (دون نفسه)
    const managed = new Set(
      me.permissions.filter((p) => p.roleName === MANAGER).map((p) => p.departmentName),
    )
    const users = mockAccounts().filter(
      (account) =>
        managed.has(ADMIN_DEPARTMENT) ||
        (!sameId(account.userId, me.userId) &&
          account.permissions.some((p) => managed.has(p.departmentName))),
    )
    return HttpResponse.json(users.map((user) => ({ id: user.userId, name: user.fullName })))
  }),
]
