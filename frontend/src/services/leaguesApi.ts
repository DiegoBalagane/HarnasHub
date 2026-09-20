import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export type LeagueType = 'Online' | 'Lan' | 'Division1' | 'Division2' | 'Other'

export interface League {
  id: string
  name: string
  season: string
  type: LeagueType
}

export interface CreateLeaguePayload {
  name: string
  season: string
  type: LeagueType
}

export const leaguesApi = {
  getLeagues: () => apiClient.get<League[]>(API_ENDPOINTS.leagues),
  createLeague: (payload: CreateLeaguePayload) => apiClient.post<League>(API_ENDPOINTS.leagues, payload),
}
