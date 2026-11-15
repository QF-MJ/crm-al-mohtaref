import { useCallback } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/stores/auth.store'
import { loginRequest } from './auth.api'
import type { LoginForm } from '../schemas/login.schema'
import type { LoginDto } from '../types'

export function useLogin() {
  const login = useAuthStore((state) => state.login)
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: LoginForm) => loginRequest(values),
    // äãÓÍ ßÇÔ ÇáãÓÊÎÏã ÇáÓÇÈÞ ÞÈá ÊËÈíÊ ÇáÌáÓÉ ÇáÌÏíÏÉ¡ ÍÊì áÇ ÊÙåÑ ÈíÇäÇÊå áÕÇÍÈ ÕáÇÍíÉ ÃÎÑì
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
