import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { rosterApi, type TeamRole, type UserRole } from '../../../services/rosterApi'
import { useAuthStore } from '../../auth/stores/useAuthStore'

/** Fetches the team roster; cached and deduped across every component that mounts it. */
export function useRoster(enabled = true) {
  return useQuery({
    queryKey: ['roster'],
    queryFn: rosterApi.getRoster,
    enabled,
  })
}

/** Changes a team member's access level (Manager only) and refreshes the roster. */
export function useUpdateRole() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: UserRole }) => rosterApi.updateRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] })
    },
  })
}

/** Assigns (or clears) a team member's in-game role — Coach/Manager only — and refreshes the roster. */
export function useUpdateTeamRole() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, teamRole }: { userId: string; teamRole: TeamRole | null }) =>
      rosterApi.updateTeamRole(userId, teamRole),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] })
    },
  })
}

/** Saves the caller's own in-game nickname and mirrors it into the session store. */
export function useUpdateOwnNickname() {
  const queryClient = useQueryClient()
  const setInGameNickname = useAuthStore((state) => state.setInGameNickname)

  return useMutation({
    mutationFn: (nickname: string) => rosterApi.updateMyNickname(nickname),
    onSuccess: (member) => {
      setInGameNickname(member.inGameNickname)
      queryClient.invalidateQueries({ queryKey: ['roster'] })
    },
  })
}
