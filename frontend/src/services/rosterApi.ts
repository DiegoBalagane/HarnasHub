import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

/** Access level inside HarnasHub — controls what a member may see and edit; `Guest` awaits a Manager's approval. Independent of `isCoach`. */
export type AccessLevel = 'Guest' | 'Player' | 'Manager'

/** In-game role a player fills in the team's setup, unrelated to the access level. */
export type TeamRole = 'IGL' | 'EntryFragger' | 'Support' | 'AWPer' | 'Lurker' | 'Rifler' | 'Bambik'

/** Where a player sits in the roster: the starting five, the bench, or a temporary stand-in (never shown in the availability calendar). */
export type RosterSlot = 'Main' | 'Bench' | 'StandIn'

/** Self-chosen map-radar pin colour, only settable by Main-roster players. */
export type PinColor = 'Orange' | 'Yellow' | 'Purple' | 'Green' | 'Blue'

export interface TeamMember {
  id: string
  displayName: string
  role: AccessLevel
  /** Whether this member is tagged as the team's Coach, independent of access level. */
  isCoach: boolean
  avatarUrl: string | null
  teamRole: TeamRole | null
  rosterSlot: RosterSlot | null
  pinColor: PinColor | null
  /** Self-chosen single character shown on the member's map-radar pin, settable by anyone; falls back to their initials when unset. */
  pinMark: string | null
  inGameNickname: string | null
  /** Backup in-game roles alongside the primary one, e.g. "second AWPer". */
  secondaryTeamRoles: TeamRole[]
}

export const rosterApi = {
  getRoster: () => apiClient.get<TeamMember[]>(API_ENDPOINTS.roster.list),
  updateRole: (userId: string, role: AccessLevel) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.role(userId), { role }),
  /** Manager only; toggles whether a member is tagged as the team's Coach, independent of access level. */
  updateIsCoach: (userId: string, isCoach: boolean) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.isCoach(userId), { isCoach }),
  updateTeamRole: (userId: string, teamRole: TeamRole | null) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.teamRole(userId), { teamRole }),
  updateRosterSlot: (userId: string, rosterSlot: RosterSlot | null) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.rosterSlot(userId), { rosterSlot }),
  /** Replaces the full set of secondary roles for that member. */
  updateSecondaryTeamRoles: (userId: string, teamRoles: TeamRole[]) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.secondaryTeamRoles(userId), { teamRoles }),
  /** A null/blank nickname clears it back to the Discord name. */
  updateMyNickname: (nickname: string | null) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.myNickname, { nickname }),
  /** Rejected server-side unless the caller is on the Main roster; a null value clears it. */
  updateMyPinColor: (pinColor: PinColor | null) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.myPinColor, { pinColor }),
  /** Manager only; rejected server-side unless the target member is on the Main roster; a null value clears it. */
  updatePinColor: (userId: string, pinColor: PinColor | null) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.pinColor(userId), { pinColor }),
  /** Open to every roster member; a null/blank value clears it back to auto-generated initials. */
  updateMyPinMark: (pinMark: string | null) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.myPinMark, { pinMark }),
  /** Manager only; permanently deletes the account and its personal data. Team artifacts they created are kept. */
  deleteMember: (userId: string) => apiClient.delete<void>(API_ENDPOINTS.roster.byId(userId)),
}
