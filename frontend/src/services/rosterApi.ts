import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

/** Access level inside HarnasHub — controls what a member may see and edit; `Guest` awaits a Manager's approval. */
export type UserRole = 'Guest' | 'Player' | 'Coach' | 'Manager'

/** In-game role a player fills in the team's setup, unrelated to the access level. */
export type TeamRole = 'IGL' | 'EntryFragger' | 'Support' | 'AWPer' | 'Lurker' | 'Rifler' | 'Bambik'

export interface TeamMember {
  id: string
  displayName: string
  role: UserRole
  avatarUrl: string | null
  teamRole: TeamRole | null
  inGameNickname: string | null
}

export const rosterApi = {
  getRoster: () => apiClient.get<TeamMember[]>(API_ENDPOINTS.roster.list),
  updateRole: (userId: string, role: UserRole) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.role(userId), { role }),
  updateTeamRole: (userId: string, teamRole: TeamRole | null) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.teamRole(userId), { teamRole }),
  updateMyNickname: (nickname: string) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.roster.myNickname, { nickname }),
}
