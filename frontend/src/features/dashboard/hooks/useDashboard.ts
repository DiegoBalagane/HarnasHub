import { useQuery } from '@tanstack/react-query'
import { dashboardApi } from '../../../services/dashboardApi'

/** Fetches the dashboard summary: next event and open task count. */
export function useDashboard() {
  return useQuery({
    queryKey: ['dashboard'],
    queryFn: dashboardApi.getSummary,
  })
}
