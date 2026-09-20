import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { tournamentsApi } from '../../../services/tournamentsApi'

/** Fetches every tournament, alphabetically. */
export function useTournaments() {
  return useQuery({
    queryKey: ['tournaments'],
    queryFn: tournamentsApi.getTournaments,
  })
}

/** Creates a tournament and refreshes the list. */
export function useCreateTournament() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (name: string) => tournamentsApi.createTournament(name),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tournaments'] })
    },
  })
}
