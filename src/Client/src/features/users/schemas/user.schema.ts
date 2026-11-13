import { z } from 'zod'

// رسائل التحقق مفاتيح ترجمة (لا نصوص عربية داخل المكونات) — انظر i18n/validation.*
export const VALIDATION = {
  required: 'validation.required',
  minLength: 'validation.minLength',
  maxLength: 'validation.maxLength',
  email: 'validation.email',
  accountName: 'validation.accountName',
  passwordMin: 'validation.passwordMin',
  passwordMax: 'validation.passwordMax',
  passwordUpper: 'validation.passwordUpper',
  passwordLower: 'validation.passwordLower',
  passwordDigit: 'validation.passwordDigit',
  passwordSpecial: 'validation.passwordSpecial',
  permissionRequired: 'validation.permissionRequired',
  duplicatePermission: 'validation.duplicatePermission',
} as const

const accountNameSchema = z
  .string()
  .trim()
  .min(1, VALIDATION.required)
  .max(50, VALIDATION.maxLength)
  .regex(/^[a-zA-Z0-9._-]+$/, VALIDATION.accountName)

const fullNameSchema = z.string().trim().min(3, VALIDATION.minLength).max(100, VALIDATION.maxLength)

const emailSchema = z.string().trim().min(1, VALIDATION.required).email(VALIDATION.email)

// نفس قواعد CreateUserCommandValidator في الباك
const passwordSchema = z
  .string()
  .min(8, VALIDATION.passwordMin)
  .max(100, VALIDATION.passwordMax)
  .regex(/[A-Z]/, VALIDATION.passwordUpper)
  .regex(/[a-z]/, VALIDATION.passwordLower)
  .regex(/[0-9]/, VALIDATION.passwordDigit)
  .regex(/[^a-zA-Z0-9]/, VALIDATION.passwordSpecial)

const permissionSchema = z.object({
  roleId: z.string().min(1, VALIDATION.required),
  departmentId: z.string().min(1, VALIDATION.required),
})

function buildUserSchema(createMode: boolean) {
  return z
    .object({
      accountName: accountNameSchema,
      fullName: fullNameSchema,
      email: emailSchema,
      isActive: z.boolean(),
      password: z.string().optional(),
      permissions: z.array(permissionSchema).min(1, VALIDATION.permissionRequired),
    })
    .superRefine((values, ctx) => {
      // كلمة المرور مطلوبة عند الإنشاء فقط
      if (createMode) {
        const result = passwordSchema.safeParse(values.password ?? '')
        if (!result.success) {
          for (const issue of result.error.issues) {
            ctx.addIssue({ code: 'custom', path: ['password'], message: issue.message })
          }
        }
      }

      // الباك يرفض الأزواج المكرّرة (فهرس فريد)، فنمنعها قبل الإرسال
      const seen = new Set<string>()
      values.permissions.forEach((permission, index) => {
        const key = `${permission.roleId}:${permission.departmentId}`
        if (seen.has(key)) {
          ctx.addIssue({
            code: 'custom',
            path: ['permissions', index, 'roleId'],
            message: VALIDATION.duplicatePermission,
          })
        }
        seen.add(key)
      })
    })
}

export const userFormSchema = buildUserSchema(false)
export const createUserSchema = buildUserSchema(true)

export type UserFormValues = z.infer<typeof userFormSchema>
