import { useQuery } from '@tanstack/react-query'
import type { MapName } from '../../../services/nadesApi'
import { tacticMatchingApi } from '../../../services/tacticMatchingApi'

/** Per-tactic effectiveness on a map (keyed under 'tactics', so any tactic edit refreshes it). */
export function useTacticEffectiveness(mapName: MapName | undefined) {
  return useQuery({
    queryKey: ['tactics', 'effectiveness', mapName],
    queryFn: () => tacticMatchingApi.getEffectiveness(mapName as MapName),
    enabled: Boolean(mapName),
  })
}

/** Automatic per-round tactic matches of one match (keyed under 'tactics', so any tactic edit refreshes it). */
export function useMatchTacticMatches(matchResultId: string, enabled = true) {
  return useQuery({
    queryKey: ['tactics', 'match-matches', matchResultId],
    queryFn: () => tacticMatchingApi.getMatchTacticMatches(matchResultId),
    enabled,
    retry: false,
  })
}
