import { z } from 'zod'
import { GUID_PATTERN } from '@/lib/guid'

const lookupIdSchema = z.union([z.string().regex(GUID_PATTERN), z.number().int().positive()])

const permissionSchema = z.object({
  roleId: lookupIdSchema,
  departmentId: lookupIdSchema,
})

export const userSchema = z.object({
  accountName: z.string().trim().min(1),
  fullName: z.string().trim().min(1).max(200),
  email: z.string().trim().email(),
  isActive: z.boolean(),
  password: z.string().optional(),
  permissions: z.array(permissionSchema).min(1),
})

export const createUserSchema = z
  .object({
    accountName: z.string().trim(),
    fullName: z.string().trim(),
    email: z.string().trim().email(),
    isActive: z.boolean(),
    password: z.string().optional(),
    permissions: z.array(permissionSchema),
  })
  .superRefine((values, context) => {
    if (
      values.accountName.length < 3 ||
      values.accountName.length > 50 ||
      !/^[a-zA-Z0-9._-]+$/.test(values.accountName)
    ) {
      context.addIssue({ code: 'custom', path: ['accountName'], message: 'invalidAccountName' })
    }

    if (values.fullName.length < 3 || values.fullName.length > 100) {
      context.addIssue({ code: 'custom', path: ['fullName'], message: 'invalidFullName' })
    }

    if (values.email.length > 150) {
      context.addIssue({ code: 'custom', path: ['email'], message: 'invalidEmail' })
    }

    const password = values.password ?? ''
    if (
      password.length < 8 ||
      password.length > 100 ||
      !/[A-Z]/.test(password) ||
      !/[a-z]/.test(password) ||
      !/[0-9]/.test(password) ||
      !/[^a-zA-Z0-9]/.test(password)
    ) {
      context.addIssue({ code: 'custom', path: ['password'], message: 'passwordRequirements' })
    }
  })

export type UserFormValues = z.infer<typeof userSchema>
