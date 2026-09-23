import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export interface Tournament {
  id: string
  name: string
}

export const tournamentsApi = {
  getTournaments: () => apiClient.get<Tournament[]>(API_ENDPOINTS.tournaments),
  createTournament: (name: string) => apiClient.post<Tournament>(API_ENDPOINTS.tournaments, { name }),
  deleteTournament: (tournamentId: string) => apiClient.delete<void>(API_ENDPOINTS.tournamentById(tournamentId)),
}
