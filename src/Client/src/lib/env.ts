export const env = {
  apiUrl: import.meta.env.VITE_API_URL ?? '/api',
  // المحاكاة تعمل في وضع التطوير فقط، حتى لا تصل للنسخة النهائية بالغلط
  useMocks: import.meta.env.DEV && import.meta.env.VITE_USE_MOCKS === 'true',
}
