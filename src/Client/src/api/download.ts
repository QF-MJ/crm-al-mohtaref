import { api } from './client'

// تنزيل ملف من الباك (تقارير PDF / Excel): FR-16, FR-17
export async function downloadFile(
  url: string,
  params?: Record<string, unknown>,
  fallbackName = 'file',
) {
  const res = await api.get<Blob>(url, { params, responseType: 'blob' })
  const disposition = res.headers['content-disposition'] as string | undefined
  const match = disposition?.match(/filename\*?=(?:UTF-8'')?"?([^";]+)"?/i)
  const filename = match ? decodeURIComponent(match[1]) : fallbackName

  const href = URL.createObjectURL(res.data)
  const link = document.createElement('a')
  link.href = href
  link.download = filename
  link.click()
  URL.revokeObjectURL(href)
}
