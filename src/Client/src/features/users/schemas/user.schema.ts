import { z } from 'zod'

const permissionSchema = z.object({
  roleId: z.number().int().positive(),
  departmentId: z.number().int().positive(),
})

export const userSchema = z.object({
  accountName: z.string().trim().min(1),
  fullName: z.string().trim().min(1).max(200),
  email: z.string().trim().email(),
  isActive: z.boolean(),
  password: z.string().optional(),
  permissions: z.array(permissionSchema).min(1),
})

export const createUserSchema = z.object({
  accountName: z.string(),
  fullName: z.string(),
  email: z.string(),
  isActive: z.boolean(),
  password: z.string().optional(),
  permissions: z.array(permissionSchema),
})

export type UserFormValues = z.infer<typeof userSchema>
