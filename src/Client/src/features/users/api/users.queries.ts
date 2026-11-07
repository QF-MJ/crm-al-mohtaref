import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createUser, getUser, getUserLookups, getUsers, updateUser } from './users.api'
import type { UserCreateInput, UserFilters, UserUpdateInput } from '../types'

const usersKey = ['users'] as const
const userLookupsKey = ['users', 'lookups'] as const

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
  return useQuery({
    queryKey: userLookupsKey,
    queryFn: ({ signal }) => getUserLookups(signal),
    staleTime: 5 * 60_000,
  })
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
    mutationFn: (body: UserCreateInput) => createUser(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: usersKey }),
  })
}
