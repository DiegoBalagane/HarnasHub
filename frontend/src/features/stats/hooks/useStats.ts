import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { MatchCategory } from '../../../services/resultsApi'
import { statsApi, type AddPlayerStatPayload } from '../../../services/statsApi'

/** Fetches every player's stat line for one match. */
export function useMatchStats(matchResultId: string, enabled: boolean) {
  return useQuery({
    queryKey: ['stats', 'match', matchResultId],
    queryFn: () => statsApi.getMatchStats(matchResultId),
    enabled,
  })
}

/** Records a player's stat line for a match and refreshes that match's stats. */
export function useAddPlayerStat(matchResultId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: AddPlayerStatPayload) => statsApi.addPlayerStat(matchResultId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stats', 'match', matchResultId] })
    },
  })
}

/** Fetches the current user's stat history across matches, for a personal trend view. */
export function useMyStatsHistory() {
  return useQuery({
    queryKey: ['stats', 'mine'],
    queryFn: statsApi.getMyStatsHistory,
  })
}

/** Fetches the team's win-rate trend across logged matches. */
export function useTeamTrend() {
  return useQuery({
    queryKey: ['stats', 'team-trend'],
    queryFn: statsApi.getTeamTrend,
  })
}

/** Fetches every roster player's stats averaged across their matches, optionally narrowed to one match category. */
export function usePlayerLeaderboard(category?: MatchCategory) {
  return useQuery({
    queryKey: ['stats', 'leaderboard', category ?? 'all'],
    queryFn: () => statsApi.getLeaderboard(category),
  })
}
