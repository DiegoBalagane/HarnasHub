import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { MapName } from '../../../services/nadesApi'
import type { MapSide } from '../../../services/mapStrategyApi'
import {
  tacticsApi,
  type CreateTacticPayload,
  type EconomyType,
  type UpdateTacticPayload,
} from '../../../services/tacticsApi'

/** Fetches saved tactics, optionally filtered by map, side and/or economy. */
export function useTactics(filters: { mapName?: MapName; side?: MapSide; economy?: EconomyType }) {
  return useQuery({
    queryKey: ['tactics', filters],
    queryFn: () => tacticsApi.getTactics(filters),
  })
}

/** Fetches one tactic's full radar layout for the editor. */
export function useTacticDetail(tacticId: string | null) {
  return useQuery({
    queryKey: ['tactics', 'detail', tacticId],
    queryFn: () => tacticsApi.getTacticDetail(tacticId!),
    enabled: tacticId !== null,
  })
}

/** Creates a new, empty tactic and refreshes the list. */
export function useCreateTactic() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateTacticPayload) => tacticsApi.createTactic(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tactics'] })
    },
  })
}

/** Saves a tactic's metadata and its whole radar layout in one call. */
export function useUpdateTactic(tacticId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: UpdateTacticPayload) => tacticsApi.updateTactic(tacticId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tactics'] })
    },
  })
}

/** Deletes a tactic and refreshes the list. */
export function useDeleteTactic() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (tacticId: string) => tacticsApi.deleteTactic(tacticId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tactics'] })
    },
  })
}
