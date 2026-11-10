import { z } from 'zod'

export const loginSchema = z.object({
  accountName: z.string().min(1),
  password: z.string().min(1),
})

// النوع يُشتق من الـ schema، فلا نكتبه مرتين
export type LoginForm = z.infer<typeof loginSchema>
