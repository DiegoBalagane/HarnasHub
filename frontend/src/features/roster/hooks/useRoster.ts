import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  rosterApi,
  type PinColor,
  type RosterSlot,
  type TeamRole,
  type UserRole,
} from '../../../services/rosterApi'
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

/** Assigns (or clears) a team member's roster slot (Main/Bench/StandIn) — Coach/Manager only — and refreshes the roster and calendar. */
export function useUpdateRosterSlot() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, rosterSlot }: { userId: string; rosterSlot: RosterSlot | null }) =>
      rosterApi.updateRosterSlot(userId, rosterSlot),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] })
      queryClient.invalidateQueries({ queryKey: ['availability'] })
    },
  })
}

/** Replaces a team member's full set of secondary (backup) in-game roles — Coach/Manager only — and refreshes the roster. */
export function useUpdateSecondaryTeamRoles() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, teamRoles }: { userId: string; teamRoles: TeamRole[] }) =>
      rosterApi.updateSecondaryTeamRoles(userId, teamRoles),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] })
    },
  })
}

/** Saves (or clears) the caller's own map-radar pin colour — rejected server-side unless they're on the Main roster. */
export function useUpdateOwnPinColor() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (pinColor: PinColor | null) => rosterApi.updateMyPinColor(pinColor),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] })
      queryClient.invalidateQueries({ queryKey: ['map-strategy'] })
    },
  })
}

/** Saves (or clears) the caller's own map-radar pin mark — open to every roster member. */
export function useUpdateOwnPinMark() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (pinMark: string | null) => rosterApi.updateMyPinMark(pinMark),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] })
      queryClient.invalidateQueries({ queryKey: ['map-strategy'] })
    },
  })
}

/** Saves the caller's own in-game nickname and mirrors it into the session store. */
export function useUpdateOwnNickname() {
  const queryClient = useQueryClient()
  const setInGameNickname = useAuthStore((state) => state.setInGameNickname)

  return useMutation({
    mutationFn: (nickname: string | null) => rosterApi.updateMyNickname(nickname),
    onSuccess: (member) => {
      setInGameNickname(member.inGameNickname)
      queryClient.invalidateQueries({ queryKey: ['roster'] })
    },
  })
}
