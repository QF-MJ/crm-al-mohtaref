import { http, HttpResponse, delay } from 'msw'
import { env } from '@/lib/env'
import type { Contact, ContactsPage } from '@/features/clients'
import type { LookupsResponse } from '@/features/lookups'
import type {
  UserCreateInput,
  UserDetail,
  UserListItem,
  UserUpdateInput,
  UsersPage,
} from '@/features/users'

// محاكاة لعقد الباك: كل المعرّفات Guid، وردود الصفحات { items, page }.
// ملاحظة: تقييد النطاق حسب القسم يتم على الباك الحقيقي؛ هنا نعيد البيانات المطابقة للفلاتر فقط.

const guid = (suffix: number) => `00000000-0000-0000-0000-${String(suffix).padStart(12, '0')}`
const PAGE_SIZE = 20

const departments = [
  { id: guid(101), name: 'المبيعات' },
  { id: guid(102), name: 'التسويق' },
  { id: guid(103), name: 'خدمة العملاء' },
  { id: guid(104), name: 'التوجيه الأكاديمي' },
  { id: guid(105), name: 'الإدارة' },
]

const roles = [
  { id: guid(201), name: 'موظف' },
  { id: guid(202), name: 'مدير' },
]

const tracks = [
  { id: guid(301), name: 'البرمجة للأطفال' },
  { id: guid(302), name: 'اللغة الإنجليزية للأطفال' },
]

const leadStatuses = [
  { id: guid(401), name: 'جديد' },
  { id: guid(402), name: 'تم التواصل' },
  { id: guid(403), name: 'مسجل' },
  { id: guid(404), name: 'غير مهتم' },
]

const tags = [
  { id: guid(501), name: 'مهتم بالبرمجة' },
  { id: guid(502), name: 'مهتم باللغة الإنجليزية' },
  { id: guid(503), name: 'العمر 6-9 سنوات' },
  { id: guid(504), name: 'العمر 10-14 سنة' },
  { id: guid(505), name: 'يتوفر جهاز' },
  { id: guid(506), name: 'لا يتوفر جهاز' },
  { id: guid(507), name: 'يرغب بالبدء فوراً' },
  { id: guid(508), name: 'عميل ساخن' },
]

const seededNames = [
  'مدير النظام',
  'أحمد خالد الحسن',
  'سارة محمد العلي',
  'عمر يوسف حداد',
  'ليلى حسام الدين',
  'محمد أنس قباني',
  'رنا سمير الخطيب',
  'يوسف عدنان درويش',
  'هبة الله مصطفى',
  'كريم وائل شاهين',
  'دانا فادي منصور',
  'طارق نزار السيد',
  'ميساء جمال ديب',
  'باسل رامي حمدان',
  'نور الهدى بكري',
  'زياد ماهر عيسى',
  'لانا عماد يونس',
  'حسين علي إبراهيم',
  'مرح سليم الأحمد',
  'فراس غسان نصر',
  'ريم أيمن سلوم',
  'ماهر بشير العمر',
  'جود طلال مراد',
  'سامر هشام عباس',
  'آلاء رياض ناصر',
]

interface MockPermission {
  id: string
  departmentId: string
  roleId: string
  departmentName: string
  roleName: string
}

interface MockUser extends UserListItem {
  permissions: MockPermission[]
}

function createPermission(
  departmentIndex: number,
  roleIndex: number,
  suffix: number,
): MockPermission {
  const department = departments[departmentIndex]
  const role = roles[roleIndex]
  return {
    id: guid(suffix),
    departmentId: department.id,
    roleId: role.id,
    departmentName: department.name,
    roleName: role.name,
  }
}

function createSeedUsers(): MockUser[] {
  return seededNames.map((fullName, index) => {
    const accountName = index === 0 ? 'admin' : `user${String(index).padStart(2, '0')}`
    // admin: مدير/الإدارة — والباقي موزعون على 4 أقسام، كل خامس موظف مدير
    const permission =
      index === 0
        ? createPermission(4, 1, 5000)
        : createPermission((index - 1) % 4, index % 5 === 0 ? 1 : 0, 5000 + index)

    return {
      id: guid(1000 + index),
      accountName,
      fullName,
      email: `${accountName}@almohtaref.local`,
      // الأدمن يبقى نشطاً دائماً حتى لا تظهر بيانات العرض بحالة غير منطقية
      isActive: index === 0 ? true : index % 7 !== 0,
      permissions: [permission],
    }
  })
}

const users = createSeedUsers()

const contactNames = [
  'أم أحمد',
  'أبو ليان',
  'هدى العلي',
  'سامي درويش',
  'ناديا الحسن',
  'أم يوسف',
  'فادي منصور',
  'ريم السيد',
  'خالد الحلبي',
  'لمى الشامي',
]

const studentNames = ['أحمد', 'ليان', 'يوسف', 'سارة', 'عمر', 'لمى', 'آدم', 'نور', 'جنى', 'زياد']

function createContacts(): Contact[] {
  return Array.from({ length: 28 }, (_, index) => {
    const studentsCount = index % 3
    return {
      id: guid(2000 + index),
      name: `${contactNames[index % contactNames.length]} ${index + 1}`,
      students: Array.from({ length: studentsCount }, (_unused, studentIndex) => ({
        id: guid(7000 + index * 3 + studentIndex),
        name: `${studentNames[(index + studentIndex) % studentNames.length]} ${
          index + 1
        }-${studentIndex + 1}`,
      })),
    }
  })
}

const contacts = createContacts()

const listItem = (user: MockUser): UserListItem => {
  const { id, accountName, fullName, email, isActive } = user
  return { id, accountName, fullName, email, isActive }
}

const userDetail = (user: MockUser): UserDetail => ({
  ...listItem(user),
  permissions: user.permissions.map(({ id, departmentName, roleName }) => ({
    id,
    departmentName,
    roleName,
  })),
})

function paginate<T>(items: T[], pageNumber: number) {
  const start = (pageNumber - 1) * PAGE_SIZE
  return {
    slice: items.slice(start, start + PAGE_SIZE),
    page: { pageNumber, hasNextPage: start + PAGE_SIZE < items.length },
  }
}

export const usersHandlers = [
  http.get(`${env.apiUrl}/lookups`, ({ request }) => {
    const include = new URL(request.url).searchParams.getAll('include')
    const includes = (value: string) => include.length === 0 || include.includes(value)

    const body: LookupsResponse = {
      departments: includes('Departments') ? departments : undefined,
      roles: includes('Roles') ? roles : undefined,
      tracks: includes('Tracks') ? tracks : undefined,
      leadStatuses: includes('LeadStatuses') ? leadStatuses : undefined,
      tags: includes('Tags') ? tags : undefined,
    }
    return HttpResponse.json(body)
  }),

  http.get(`${env.apiUrl}/users`, async ({ request }) => {
    await delay(200)
    const params = new URL(request.url).searchParams
    const pageNumber = Math.max(1, Number(params.get('pageNumber')) || 1)
    const name = params.get('name')?.trim().toLocaleLowerCase('ar')
    const accountName = params.get('accountName')?.trim().toLocaleLowerCase('ar')
    const email = params.get('email')?.trim().toLocaleLowerCase('ar')
    const isActiveValue = params.get('isActive')
    const isActive = isActiveValue === null ? undefined : isActiveValue.toLowerCase() === 'true'
    const roleId = params.get('roleId') ?? undefined
    const departmentId = params.get('departmentId') ?? undefined

    const filtered = users.filter(
      (user) =>
        (!name || user.fullName.toLocaleLowerCase('ar').includes(name)) &&
        (!accountName || user.accountName.toLocaleLowerCase('ar').includes(accountName)) &&
        (!email || user.email.toLocaleLowerCase('ar').includes(email)) &&
        (isActive === undefined || user.isActive === isActive) &&
        (!roleId && !departmentId
          ? true
          : user.permissions.some(
              (permission) =>
                (!roleId || permission.roleId === roleId) &&
                (!departmentId || permission.departmentId === departmentId),
            )),
    )

    const { slice, page } = paginate(filtered, pageNumber)
    const body: UsersPage = { items: slice.map(listItem), page }
    return HttpResponse.json(body)
  }),

  http.get(`${env.apiUrl}/users/:id`, ({ params }) => {
    const user = users.find((item) => item.id === params.id)
    return user
      ? HttpResponse.json(userDetail(user))
      : HttpResponse.json({ status: 404, message: 'المستخدم غير موجود' }, { status: 404 })
  }),

  http.post(`${env.apiUrl}/users`, async ({ request }) => {
    const body = (await request.json()) as UserCreateInput
    await delay(250)
    const accountName = body.accountName.trim()
    const email = body.email.trim()

    if (
      users.some(
        (user) =>
          user.accountName.toLocaleLowerCase('ar') === accountName.toLocaleLowerCase('ar') ||
          user.email.toLocaleLowerCase('ar') === email.toLocaleLowerCase('ar'),
      )
    ) {
      return HttpResponse.json(
        { status: 409, message: 'البريد الإلكتروني أو اسم الحساب مستخدم مسبقاً' },
        { status: 409 },
      )
    }

    const permissions: MockPermission[] = []
    for (const [index, input] of (body.permissions ?? []).entries()) {
      const departmentIndex = departments.findIndex((item) => item.id === input.departmentId)
      const roleIndex = roles.findIndex((item) => item.id === input.roleId)
      if (departmentIndex < 0 || roleIndex < 0) {
        return HttpResponse.json({ status: 400, message: 'قسم أو دور غير صالح' }, { status: 400 })
      }
      permissions.push(createPermission(departmentIndex, roleIndex, 6000 + index))
    }

    const id = guid(users.length + 1)
    users.push({
      id,
      accountName,
      fullName: body.fullName.trim(),
      email,
      isActive: true,
      permissions,
    })
    return HttpResponse.json(id, { status: 201 })
  }),

  http.put(`${env.apiUrl}/users/:id`, async ({ params, request }) => {
    const user = users.find((item) => item.id === params.id)
    if (!user) {
      return HttpResponse.json({ status: 404, message: 'المستخدم غير موجود' }, { status: 404 })
    }

    const body = (await request.json()) as UserUpdateInput
    await delay(250)
    user.accountName = body.accountName
    user.fullName = body.fullName
    user.email = body.email
    if (body.isActive !== undefined && body.isActive !== null) {
      user.isActive = body.isActive
    }

    const permissions: MockPermission[] = []
    for (const [index, permission] of body.permissions.entries()) {
      const departmentIndex = departments.findIndex((item) => item.id === permission.departmentId)
      const roleIndex = roles.findIndex((item) => item.id === permission.roleId)
      if (departmentIndex < 0 || roleIndex < 0) {
        return HttpResponse.json({ status: 400, message: 'قسم أو دور غير صالح' }, { status: 400 })
      }
      permissions.push(createPermission(departmentIndex, roleIndex, 6100 + index))
    }
    user.permissions = permissions
    return new HttpResponse(null, { status: 204 })
  }),
]

export const contactsHandlers = [
  http.get(`${env.apiUrl}/contacts`, async ({ request }) => {
    await delay(200)
    const params = new URL(request.url).searchParams
    const pageNumber = Math.max(1, Number(params.get('pageNumber')) || 1)
    const contactName = params.get('contactName')?.trim()
    const studentName = params.get('studentName')?.trim()

    const filtered = contacts.filter(
      (contact) =>
        (!contactName || contact.name.includes(contactName)) &&
        (!studentName || contact.students.some((student) => student.name.includes(studentName))),
    )

    const { slice, page } = paginate(filtered, pageNumber)
    const body: ContactsPage = { items: slice, page }
    return HttpResponse.json(body)
  }),
]
