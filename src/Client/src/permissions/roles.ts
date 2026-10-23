// أسماء الأدوار هنا للواجهة. تأكد من مطابقتها لما يرسله الباك (mapping عند تسجيل الدخول)
export type Role =
  | 'employee'
  | 'salesEmployee'
  | 'salesManager'
  | 'marketingEmployee'
  | 'marketingManager'
  | 'academicAdvisor'
  | 'customerService'
  | 'admin'

// نفس سلسلة الوراثة بمخطط حالات الاستخدام
export const ROLE_INHERITS: Record<Role, Role[]> = {
  employee: [],
  salesEmployee: ['employee'],
  salesManager: ['salesEmployee'],
  marketingEmployee: ['employee'],
  marketingManager: ['marketingEmployee'],
  academicAdvisor: ['employee'],
  customerService: ['employee'],
  admin: ['salesManager', 'marketingManager', 'academicAdvisor', 'customerService'],
}
