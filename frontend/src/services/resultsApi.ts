import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { LeagueType } from './leaguesApi'

export type MatchCategory = 'Scrimmage' | 'League' | 'Tournament'

export interface MatchResult {
  id: string
  opponent: string
  ourScore: number
  opponentScore: number
  mapName: string | null
  demoUrl: string | null
  notes: string | null
  playedAtUtc: string
  category: MatchCategory
  tournamentId: string | null
  tournamentName: string | null
  leagueId: string | null
  leagueName: string | null
  leagueSeason: string | null
  leagueType: LeagueType | null
}

export interface AddResultPayload {
  opponent: string
  ourScore: number
  opponentScore: number
  mapName?: string
  demoUrl?: string
  notes?: string
  playedAtUtc: string
  category: MatchCategory
  tournamentId?: string | null
  leagueId?: string | null
}

export const resultsApi = {
  getResults: () => apiClient.get<MatchResult[]>(API_ENDPOINTS.results),
  addResult: (payload: AddResultPayload) => apiClient.post<MatchResult>(API_ENDPOINTS.results, payload),
}
