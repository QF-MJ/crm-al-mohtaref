import { z } from 'zod'

export const userSchema = z.object({
  accountName: z.string().trim().min(1),
  fullName: z.string().trim().min(1),
  email: z.string().trim().email(),
  isActive: z.boolean(),
  permissions: z.array(
    z.object({
      roleId: z.number().int().positive(),
      departmentId: z.number().int().positive(),
    }),
  ),
})

export type UserFormValues = z.infer<typeof userSchema>
