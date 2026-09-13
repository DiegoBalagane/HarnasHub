import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export type TeamRole = 'Player' | 'Coach' | 'Manager'

export interface TeamMember {
  id: string
  displayName: string
  role: TeamRole
  avatarUrl: string | null
}

export const rosterApi = {
  getRoster: () => apiClient.get<TeamMember[]>(API_ENDPOINTS.roster),
  updateRole: (userId: string, role: TeamRole) =>
    apiClient.patch<TeamMember>(API_ENDPOINTS.rosterRole(userId), { role }),
}
