import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { materialsApi, type AddMaterialPayload } from '../../../services/materialsApi'

/** Fetches every training material, most recently added first. */
export function useMaterials() {
  return useQuery({
    queryKey: ['materials'],
    queryFn: materialsApi.getMaterials,
  })
}

/** Adds a training material and refreshes the list. */
export function useAddMaterial() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: AddMaterialPayload) => materialsApi.addMaterial(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['materials'] })
    },
  })
}
