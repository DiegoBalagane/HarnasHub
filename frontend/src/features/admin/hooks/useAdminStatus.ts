import { useQuery } from '@tanstack/react-query'
import { adminApi } from '../../../services/adminApi'

/** Fetches the integration configuration flags (Manager only). */
export function useAdminStatus() {
  return useQuery({ queryKey: ['admin-status'], queryFn: adminApi.getStatus })
}
