import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { vetoApi, type VetoStepInput } from '../../../services/vetoApi'

/** Fetches the suggested picks/bans against an opponent. */
export function useVetoSuggestion(opponent: string) {
  return useQuery({
    queryKey: ['veto', 'suggestion', opponent.trim().toLowerCase()],
    queryFn: () => vetoApi.getSuggestion(opponent),
    enabled: opponent.trim() !== '',
  })
}

/** Fetches the veto recorded for an event. */
export function useEventVeto(eventId: string) {
  return useQuery({
    queryKey: ['veto', 'event', eventId],
    queryFn: () => vetoApi.getEventVeto(eventId),
  })
}

/** Replaces an event's veto; also refreshes suggestions, since the opponent's tendencies come from recorded vetoes. */
export function useSetEventVeto() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ eventId, steps }: { eventId: string; steps: VetoStepInput[] }) =>
      vetoApi.setEventVeto(eventId, steps),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['veto'] })
    },
  })
}
