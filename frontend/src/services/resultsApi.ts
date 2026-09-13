import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export interface MatchResult {
  id: string
  opponent: string
  ourScore: number
  opponentScore: number
  mapName: string | null
  demoUrl: string | null
  notes: string | null
  playedAtUtc: string
}

export interface AddResultPayload {
  opponent: string
  ourScore: number
  opponentScore: number
  mapName?: string
  demoUrl?: string
  notes?: string
  playedAtUtc: string
}

export const resultsApi = {
  getResults: () => apiClient.get<MatchResult[]>(API_ENDPOINTS.results),
  addResult: (payload: AddResultPayload) => apiClient.post<MatchResult>(API_ENDPOINTS.results, payload),
}
