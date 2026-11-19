// عنوان الـAPI: يفضّل تمرير الطلبات عبر Vite proxy (/api) لتفادي مشاكل شهادة HTTPS المحلية.
// يمكن وضع عنوان كامل مثل https://localhost:7271 أو https://localhost:7271/api.
export const env = {
  apiUrl: import.meta.env.VITE_API_URL ?? '/api',
  // المحاكاة تعمل في وضع التطوير فقط، حتى لا تصل للنسخة النهائية بالغلط
  useMocks: import.meta.env.DEV && import.meta.env.VITE_USE_MOCKS === 'true',
}
