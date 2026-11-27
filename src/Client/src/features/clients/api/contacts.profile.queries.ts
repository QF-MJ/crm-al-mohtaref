import { useQuery } from '@tanstack/react-query'
import { getContactProfile } from './contacts.profile.api'

export function useContactProfile(id?: string) {
  return useQuery({
    queryKey: ['contacts', 'profile', id],
    queryFn: ({ signal }) => getContactProfile(id as string, signal),
    enabled: Boolean(id),
    // البيانات للقراءة فقط؛ إعادة الجلب عند التركيز لا تضيف قيمة وتسبب وميضاً
    refetchOnWindowFocus: false,
    retry: false,
  })
}
