import { useQuery } from '@tanstack/react-query'
import { advancedStatsApi } from '../../../services/advancedStatsApi'
import type { MatchCategory } from '../../../services/resultsApi'

/** Fetches the demo-derived advanced stats, narrowed by category, map and the newest N matches. */
export function useAdvancedStats(category?: MatchCategory, map?: string, last?: number) {
  return useQuery({
    queryKey: ['stats', 'advanced', category ?? 'all', map ?? 'all', last ?? 'all'],
    queryFn: () => advancedStatsApi.get(category, map, last),
  })
}
