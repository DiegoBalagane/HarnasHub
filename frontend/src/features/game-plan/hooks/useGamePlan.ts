import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { gamePlanApi, type SetGamePlanPayload } from '../../../services/gamePlanApi'

/** Fetches the game plan of an event. */
export function useGamePlan(eventId: string) {
  return useQuery({
    queryKey: ['game-plan', eventId],
    queryFn: () => gamePlanApi.getPlan(eventId),
  })
}

/** Replaces an event's game plan (Coach/Manager). */
export function useSetGamePlan() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ eventId, payload }: { eventId: string; payload: SetGamePlanPayload }) =>
      gamePlanApi.setPlan(eventId, payload),
    onSuccess: (plan) => {
      queryClient.setQueryData(['game-plan', plan.eventId], plan)
    },
  })
}
