import axios from 'axios'
import { env } from '@/lib/env'

const configuredApiUrl = env.apiUrl.replace(/\/+$/, '')
const apiBaseUrl = configuredApiUrl.endsWith('/api') ? configuredApiUrl : `${configuredApiUrl}/api`

// نسخة axios الوحيدة بالمشروع
export const api = axios.create({
  baseURL: apiBaseUrl,
  headers: { 'Content-Type': 'application/json' },
  timeout: 20_000,
})
