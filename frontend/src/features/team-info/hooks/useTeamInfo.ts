import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { teamInfoApi, type TeamInfoPayload } from '../../../services/teamInfoApi'

const queryKey = ['team-info']

/** Every team info entry, ordered by category and manual order. */
export function useTeamInfo() {
  return useQuery({ queryKey, queryFn: teamInfoApi.list })
}

/** Creates an entry (Coach/Manager) and refreshes the list. */
export function useCreateTeamInfo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: TeamInfoPayload) => teamInfoApi.create(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey }),
  })
}

/** Edits an entry (Coach/Manager) and refreshes the list. */
export function useUpdateTeamInfo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: TeamInfoPayload }) => teamInfoApi.update(id, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey }),
  })
}

/** Deletes an entry (Coach/Manager) and refreshes the list. */
export function useDeleteTeamInfo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => teamInfoApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey }),
  })
}

/** Saves a new order for one category's entries (Coach/Manager) and refreshes the list. */
export function useReorderTeamInfo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (orderedIds: string[]) => teamInfoApi.reorder(orderedIds),
    onSuccess: () => queryClient.invalidateQueries({ queryKey }),
  })
}
