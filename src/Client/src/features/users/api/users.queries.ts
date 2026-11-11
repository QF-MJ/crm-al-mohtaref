import { useMemo } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { addUser, getUser, getUsers, updateUser } from './users.api'
import { useLookups } from '@/features/lookups/api/lookups.queries'
import type { UserCreateInput, UserFilters, UserUpdateInput } from '../types'

const usersKey = ['users'] as const

export function useUsers(filters: UserFilters) {
  return useQuery({
    queryKey: [...usersKey, filters],
    queryFn: ({ signal }) => getUsers(filters, signal),
  })
}

export function useUser(id: number | undefined) {
  return useQuery({
    queryKey: [...usersKey, 'detail', id],
    queryFn: ({ signal }) => {
      if (id === undefined) throw new Error('A user ID is required')
      return getUser(id, signal)
    },
    enabled: id !== undefined,
  })
}

export function useUserLookups() {
  const query = useLookups()
  const data = useMemo(
    () => query.data
      ? {
          departments: query.data.departments ?? [],
          roles: query.data.roles ?? [],
        }
      : undefined,
    [query.data],
  )
  return {
    ...query,
    data,
  }
}

export function useUpdateUser(id: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: UserUpdateInput) => updateUser(id, body),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: usersKey }),
        queryClient.invalidateQueries({ queryKey: [...usersKey, 'detail', id] }),
      ]),
  })
}

export function useCreateUser() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: UserCreateInput) => addUser(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: usersKey }),
  })
}
