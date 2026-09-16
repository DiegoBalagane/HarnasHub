import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { mapStrategyApi, type MapSide, type SetPlayerPositionPayload } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'

/** Fetches every player's starting spot for one map and side. */
export function useMapPositions(mapName: MapName, side: MapSide) {
  return useQuery({
    queryKey: ['map-strategy', mapName, side],
    queryFn: () => mapStrategyApi.getPositions(mapName, side),
  })
}

/** Upserts a player's starting spot (Coach/Manager only) and refreshes the radar. */
export function useSetPlayerPosition() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: SetPlayerPositionPayload) => mapStrategyApi.setPosition(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['map-strategy'] })
    },
  })
}

/** Removes a player's starting spot (Coach/Manager only) and refreshes the radar. */
export function useRemovePlayerPosition() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (positionId: string) => mapStrategyApi.removePosition(positionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['map-strategy'] })
    },
  })
}
