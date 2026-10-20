import axios from 'axios'
import { env } from '@/lib/env'

// نسخة axios الوحيدة بالمشروع
export const api = axios.create({
  baseURL: env.apiUrl,
  headers: { 'Content-Type': 'application/json' },
})
