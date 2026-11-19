import type { Role } from './roles'

// الشكل الذي يرجّعه الباك بـ permissions عند تسجيل الدخول (camelCase كما بالرد الفعلي)
export interface PermissionPair {
  departmentName: string
  roleName: string
}

// هذه الأسماء يجب أن تطابق حرفياً Data\Seed\DepartmentSeed.cs و RoleSeed.cs بالباك
const EMPLOYEE = 'موظف'
const MANAGER = 'مدير'

const DEPARTMENT_ROLES: Record<string, { employee: Role; manager: Role }> = {
  المبيعات: { employee: 'salesEmployee', manager: 'salesManager' },
  التسويق: { employee: 'marketingEmployee', manager: 'marketingManager' },
  // مؤقت: لا يوجد دور "مدير" لهذين القسمين بـ roles.ts بعد، فنعطيه صلاحيات الموظف
  'خدمة العملاء': { employee: 'customerService', manager: 'customerService' },
  'التوجيه الأكاديمي': { employee: 'academicAdvisor', manager: 'academicAdvisor' },
}

// يحوّل أزواج (قسم + دور) القادمة من الباك لأدوار الفرونت. أي زوج غير معروف يُتجاهل
export function mapRoles(pairs: readonly PermissionPair[]): Role[] {
  const roles = new Set<Role>()
  for (const { departmentName, roleName } of pairs) {
    const entry = DEPARTMENT_ROLES[departmentName]
    if (!entry) continue
    if (roleName === EMPLOYEE) roles.add(entry.employee)
    else if (roleName === MANAGER) roles.add(entry.manager)
  }
  return [...roles]
}
