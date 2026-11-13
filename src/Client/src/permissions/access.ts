// نموذج الصلاحيات مطابق لما يفرضه الباك فعلياً:
// المستخدم يملك أزواج (قسم + دور)، والقرار يتخذ من هذه الأزواج نفسها
// كما في AuthorizePermisionsAttribute و GetUsersQueryHandler و GetContactsQuery.
//
// لا نخترع أدواراً إضافية: أسماء الأقسام والأدوار أدناه يجب أن تطابق
// SeedRules.Lookups في الباك حرفياً.

export const DEPARTMENT = {
  administration: 'الإدارة',
  sales: 'المبيعات',
  marketing: 'التسويق',
  customerService: 'خدمة العملاء',
  academic: 'التوجيه الأكاديمي',
} as const

export const ROLE = {
  employee: 'موظف',
  manager: 'مدير',
} as const

export type DepartmentName = (typeof DEPARTMENT)[keyof typeof DEPARTMENT]
export type RoleName = (typeof ROLE)[keyof typeof ROLE]

// شكل PermissionDto في رد تسجيل الدخول / تفاصيل المستخدم (id = Guid)
export interface PermissionPair {
  id: string
  departmentName: string
  roleName: string
}

// قدرات الواجهة. كل قدرة مربوطة بقاعدة موجودة في الباك وليست تخمينية.
export type Capability =
  // بيانات الحساب الشخصي: GET /api/users/{id} مسموح لصاحب الحساب وللمدراء
  | 'account.view'
  // GET /api/users: أي مدير في أي قسم (والباك يقصر النطاق على قسمه)
  | 'users.view'
  // POST /api/users و PUT /api/users/{id}: مدير الإدارة فقط
  | 'users.create'
  | 'users.update'
  // GET /api/contacts: مدير الإدارة أو مدير المبيعات فقط
  | 'contacts.view'
  | 'contacts.filter'
  // وحدات لم تُطلق في الباك بعد: تبقى صفحاتها التوضيحية متاحة بدل 403
  | 'modules.preview'

export interface Access {
  pairs: PermissionPair[]
  departments: string[]
  managerDepartments: string[]
  isAdmin: boolean
  isManager: boolean
  inAdministration: boolean
  can: (capability: Capability) => boolean
}

const normalize = (value: string) => value.trim().replace(/\s+/g, ' ')

const isManagerPair = (pair: PermissionPair) => normalize(pair.roleName) === ROLE.manager

const isPairOf = (pair: PermissionPair, department: string) =>
  normalize(pair.departmentName) === department

const unique = (values: string[]) => [...new Set(values.filter(Boolean))]

export function buildAccess(pairs: readonly PermissionPair[]): Access {
  const list = [...pairs]
  const departments = unique(list.map((pair) => normalize(pair.departmentName)))
  const managerDepartments = unique(
    list.filter(isManagerPair).map((pair) => normalize(pair.departmentName)),
  )

  const isAdmin = list.some(
    (pair) => isPairOf(pair, DEPARTMENT.administration) && isManagerPair(pair),
  )
  const isManager = managerDepartments.length > 0
  const inAdministration = departments.includes(DEPARTMENT.administration)
  const isSalesManager = managerDepartments.includes(DEPARTMENT.sales)

  const can = (capability: Capability): boolean => {
    switch (capability) {
      case 'account.view':
      case 'modules.preview':
        return true
      case 'users.view':
        // قرار المنتج: قائمة الموظفين والصلاحيات للأدمن فقط (مدير + الإدارة).
        // الباك يسمح تقنياً لأي "مدير" بجلب موظفي قسمه فقط؛
        // لإرجاع صلاحية مدير القسم لاحقاً: استبدل isAdmin بـ isManager هنا.
        return isAdmin
      case 'users.create':
      case 'users.update':
        return isAdmin
      case 'contacts.view':
      case 'contacts.filter':
        return isAdmin || isSalesManager
      default:
        return false
    }
  }

  return {
    pairs: list,
    departments,
    managerDepartments,
    isAdmin,
    isManager,
    inAdministration,
    can,
  }
}
