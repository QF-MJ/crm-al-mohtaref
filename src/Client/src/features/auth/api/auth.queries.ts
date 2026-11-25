import { useCallback } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/stores/auth.store'
import { loginRequest } from './auth.api'
import type { LoginForm } from '../schemas/login.schema'

export function useLogin() {
  const login = useAuthStore((state) => state.login)
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: LoginForm) => loginRequest(values),
    // نمسح كاش المستخدم السابق قبل تثبيت الجلسة الجديدة، حتى لا تظهر بياناته لصاحب صلاحية أخرى
    onSuccess: (dto) => {
      queryClient.clear()
      login(dto)
    },
  })
}

export function useLogout() {
  const logout = useAuthStore((state) => state.logout)
  const queryClient = useQueryClient()

  return useCallback(() => {
    queryClient.clear()
    logout()
  }, [logout, queryClient])
}
