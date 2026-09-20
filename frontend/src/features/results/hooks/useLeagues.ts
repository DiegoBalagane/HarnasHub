import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { leaguesApi, type CreateLeaguePayload } from '../../../services/leaguesApi'

/** Fetches every league season, most recently created first. */
export function useLeagues() {
  return useQuery({
    queryKey: ['leagues'],
    queryFn: leaguesApi.getLeagues,
  })
}

/** Creates a league season and refreshes the list. */
export function useCreateLeague() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateLeaguePayload) => leaguesApi.createLeague(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leagues'] })
    },
  })
}
