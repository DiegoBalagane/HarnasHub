import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  nadesApi,
  type AddNadePayload,
  type GrenadeType,
  type MapName,
  type UpdateNadePositionPayload,
} from '../../../services/nadesApi'

/** Fetches nade entries, optionally filtered by map and/or grenade type. */
export function useNades(filters: { mapName?: MapName; type?: GrenadeType }) {
  return useQuery({
    queryKey: ['nades', filters],
    queryFn: () => nadesApi.getNades(filters),
  })
}

/** Adds a nade entry and refreshes the library. */
export function useAddNade() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: AddNadePayload) => nadesApi.addNade(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nades'] })
    },
  })
}

/** Deletes a nade entry and refreshes the library. */
export function useDeleteNade() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (nadeId: string) => nadesApi.deleteNade(nadeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nades'] })
    },
  })
}

/** Sets or clears a nade entry's landing-spot pin on its map radar. */
export function useUpdateNadePosition() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: UpdateNadePositionPayload) => nadesApi.updateNadePosition(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nades'] })
    },
  })
}
