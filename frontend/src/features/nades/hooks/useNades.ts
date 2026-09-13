import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { nadesApi, type AddNadePayload, type GrenadeType } from '../../../services/nadesApi'

/** Fetches nade entries, optionally filtered by map and/or grenade type. */
export function useNades(filters: { mapName?: string; type?: GrenadeType }) {
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
