import { http, HttpResponse } from 'msw'
import type {
  LookupItem,
  UserDetail,
  UserListItem,
  UserUpdateInput,
  UsersPage,
} from '@/features/users/types'

interface MockLookupItem extends LookupItem {
  id: number
}

const departments: MockLookupItem[] = [
  { id: 1, name: 'المبيعات' },
  { id: 2, name: 'التسويق' },
  { id: 3, name: 'خدمة العملاء' },
  { id: 4, name: 'التوجيه الأكاديمي' },
  { id: 5, name: 'الإدارة' },
]

const roles: MockLookupItem[] = [
  { id: 1, name: 'موظف' },
  { id: 2, name: 'مدير' },
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
  id: number
  departmentId: number
  roleId: number
  departmentName: string
  roleName: string
}

interface MockUser extends UserDetail {
  permissions: MockPermission[]
}

function createSeedUsers(): MockUser[] {
  return seededNames.map((fullName, index) => {
    const accountName = index === 0 ? 'admin' : `user${String(index).padStart(2, '0')}`
    const userPermissions: MockPermission[] = []

    if (index === 0) {
      userPermissions.push({
        id: 1,
        departmentId: 5,
        roleId: 2,
        departmentName: departments[4].name,
        roleName: roles[1].name,
      })
    } else {
      const departmentCount = departments.length - 1
      const departmentId = ((index - 1) % departmentCount) + 1
      const roleId = index % 5 === 0 ? 2 : 1
      const department = departments[departmentId - 1]
      const role = roles[roleId - 1]
      userPermissions.push({
        id: userPermissions.length + 1,
        departmentId,
        roleId,
        departmentName: department.name,
        roleName: role.name,
      })

      if (index % 6 === 0) {
        const secondDepartmentId = (index % departmentCount) + 1
        const secondDepartment = departments[secondDepartmentId - 1]
        userPermissions.push({
          id: userPermissions.length + 1,
          departmentId: secondDepartmentId,
          roleId: 1,
          departmentName: secondDepartment.name,
          roleName: roles[0].name,
        })
      }
    }

    return {
      id: index + 1,
      accountName,
      fullName,
      email: `${accountName}@almohtaref.local`,
      isActive: true,
      permissions: userPermissions,
    }
  })
}

const users = createSeedUsers()
const pageSize = 20

function matchesPermission(user: MockUser, roleId?: number, departmentId?: number) {
  if (roleId === undefined && departmentId === undefined) return true
  return user.permissions.some(
    (permission) =>
      (roleId === undefined || permission.roleId === roleId) &&
      (departmentId === undefined || permission.departmentId === departmentId),
  )
}

function listItem(user: MockUser): UserListItem {
  const { id, accountName, fullName, email, isActive } = user
  return { id, accountName, fullName, email, isActive }
}

function userDetail(user: MockUser): UserDetail {
  return {
    ...listItem(user),
    permissions: user.permissions.map(({ id, departmentName, roleName }) => ({
      id,
      departmentName,
      roleName,
    })),
  }
}

export const usersHandlers = [
  http.get('*/api/lookups', () => {
    return HttpResponse.json({
      departments,
      roles,
      tracks: [],
      leadStatuses: [],
    })
  }),

  http.get('*/api/users', ({ request }) => {
    const params = new URL(request.url).searchParams
    const pageNumber = Math.max(1, Number(params.get('pageNumber')) || 1)
    const name = params.get('name')?.trim().toLocaleLowerCase()
    const accountName = params.get('accountName')?.trim().toLocaleLowerCase()
    const email = params.get('email')?.trim().toLocaleLowerCase()
    const isActiveValue = params.get('isActive')
    const isActive = isActiveValue === null ? undefined : isActiveValue.toLowerCase() === 'true'
    const roleId = Number(params.get('roleId')) || undefined
    const departmentId = Number(params.get('departmentId')) || undefined
    const filtered = users.filter(
      (user) =>
        (!name || user.fullName.toLocaleLowerCase().includes(name)) &&
        (!accountName || user.accountName.toLocaleLowerCase().includes(accountName)) &&
        (!email || user.email.toLocaleLowerCase().includes(email)) &&
        (isActive === undefined || user.isActive === isActive) &&
        matchesPermission(user, roleId, departmentId),
    )
    const start = (pageNumber - 1) * pageSize
    const page: UsersPage = {
      items: filtered.slice(start, start + pageSize).map(listItem),
      page: {
        pageNumber,
        hasNextPage: start + pageSize < filtered.length,
      },
    }
    return HttpResponse.json(page)
  }),

  http.get('*/api/users/:id', ({ params }) => {
    const id = Number(params.id)
    const user = users.find((item) => item.id === id)
    return user
      ? HttpResponse.json(userDetail(user))
      : HttpResponse.json({ status: 404, message: 'User not found' }, { status: 404 })
  }),

  http.post('*/api/users', async ({ request }) => {
    const body = (await request.json()) as UserCreateInput
    const accountName = body.accountName.trim()
    const email = body.email.trim()

    if (
      users.some(
        (user) =>
          user.accountName.toLocaleLowerCase() === accountName.toLocaleLowerCase() ||
          user.email.toLocaleLowerCase() === email.toLocaleLowerCase(),
      )
    ) {
      return HttpResponse.json(
        { message: 'Account name or email is already in use' },
        { status: 409 },
      )
    }

    const permissions: MockPermission[] = []
    for (const [index, input] of (body.permissions ?? []).entries()) {
      const department = departments.find((item) => item.id === input.departmentId)
      const role = roles.find((item) => item.id === input.roleId)
      if (!department || !role) {
        return HttpResponse.json({ message: 'Invalid role or department' }, { status: 400 })
      }
      permissions.push({
        id: index + 1,
        departmentId: department.id,
        roleId: role.id,
        departmentName: department.name,
        roleName: role.name,
      })
    }

    const user: MockUser = {
      id: Math.max(...users.map((item) => item.id)) + 1,
      accountName,
      fullName: body.fullName.trim(),
      email,
      isActive: true,
      permissions,
    }
    users.push(user)
    return HttpResponse.json(userDetail(user), { status: 201 })
  }),

  http.put('*/api/users/:id', async ({ params, request }) => {
    const id = Number(params.id)
    const user = users.find((item) => item.id === id)
    if (!user) return HttpResponse.json({ status: 404, message: 'User not found' }, { status: 404 })

    const body = (await request.json()) as UserUpdateInput
    user.accountName = body.accountName
    user.fullName = body.fullName
    user.email = body.email
    if (body.isActive !== undefined && body.isActive !== null) {
      user.isActive = body.isActive
    }
    const updatedPermissions: MockPermission[] = []
    for (const [index, permission] of body.permissions.entries()) {
      const department = departments.find((item) => item.id === permission.departmentId)
      const role = roles.find((item) => item.id === permission.roleId)
      if (!department || !role) {
        return HttpResponse.json(
          { status: 400, message: 'Unknown department or role ID in user update' },
          { status: 400 },
        )
      }
      updatedPermissions.push({
        id: index + 1,
        departmentId: department.id,
        roleId: role.id,
        departmentName: department.name,
        roleName: role.name,
      })
    }
    user.permissions = updatedPermissions
    return new HttpResponse(null, { status: 204 })
  }),
]
