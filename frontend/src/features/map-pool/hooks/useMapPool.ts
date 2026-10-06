import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { mapPoolApi, type SetMapPoolEntryPayload } from '../../../services/mapPoolApi'
import type { MapName } from '../../../services/nadesApi'
import type { MatchCategory } from '../../../services/resultsApi'

/** Fetches every map of the pool with its status and record, optionally counting only one match category. */
export function useMapPool(category?: MatchCategory) {
  return useQuery({
    queryKey: ['map-pool', category ?? 'all'],
    queryFn: () => mapPoolApi.getMapPool(category),
  })
}

/** Sets or clears a map's place in the pool (Coach/Manager). */
export function useSetMapPoolEntry() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ mapName, payload }: { mapName: MapName; payload: SetMapPoolEntryPayload }) =>
      mapPoolApi.setEntry(mapName, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['map-pool'] })
    },
  })
}
