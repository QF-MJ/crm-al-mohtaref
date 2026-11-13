import { QueryClient } from '@tanstack/react-query'

// نسخة واحدة مشتركة: نمسحها عند الخروج/تسجيل الدخول حتى لا تُعرض بيانات جلسة سابقة
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})
