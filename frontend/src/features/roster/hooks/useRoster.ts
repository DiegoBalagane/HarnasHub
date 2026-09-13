import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { rosterApi, type TeamRole } from '../../../services/rosterApi'

/** Fetches the team roster; cached and deduped across every component that mounts it. */
export function useRoster() {
  return useQuery({
    queryKey: ['roster'],
    queryFn: rosterApi.getRoster,
  })
}

/** Changes a team member's role (Manager only) and refreshes the roster. */
export function useUpdateRole() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: TeamRole }) => rosterApi.updateRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] })
    },
  })
}
