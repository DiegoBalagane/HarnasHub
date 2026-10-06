import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { replayApi, type ReplaySource } from '../../../services/replayApi'

/** Fetches the 2D replay of one round of a match or an opponent demo. */
export function useRoundReplay(source: ReplaySource, sourceId: string, roundNumber: number) {
  return useQuery({
    queryKey: ['round-replay', source, sourceId, roundNumber],
    queryFn: () =>
      source === 'Match'
        ? replayApi.getMatchRound(sourceId, roundNumber)
        : replayApi.getOpponentDemoRound(sourceId, roundNumber),
  })
}

/** Saves the replayed round at a given second as a new analysis board. */
export function useCreateBoardFromRound(source: ReplaySource, sourceId: string, roundNumber: number) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (second: number) => replayApi.createBoardFromRound({ source, sourceId, roundNumber, second }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['analysis-boards'] })
    },
  })
}
