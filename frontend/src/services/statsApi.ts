import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export interface PlayerMatchStat {
  id: string
  userId: string
  displayName: string
  kills: number
  deaths: number
  assists: number
  adr: number
  headshotPercentage: number
  rating: number
}

export interface AddPlayerStatPayload {
  userId: string
  kills: number
  deaths: number
  assists: number
  adr: number
  headshotPercentage: number
  rating: number
}

export interface PlayerStatHistoryEntry {
  matchResultId: string
  playedAtUtc: string
  opponent: string
  kills: number
  deaths: number
  assists: number
  adr: number
  headshotPercentage: number
  rating: number
}

export interface TeamTrendPoint {
  playedAtUtc: string
  won: boolean
  cumulativeWins: number
  cumulativeLosses: number
  winRatePercentage: number
}

export const statsApi = {
  getMatchStats: (matchResultId: string) =>
    apiClient.get<PlayerMatchStat[]>(API_ENDPOINTS.matchStats(matchResultId)),
  addPlayerStat: (matchResultId: string, payload: AddPlayerStatPayload) =>
    apiClient.post<PlayerMatchStat>(API_ENDPOINTS.matchStats(matchResultId), payload),
  getMyStatsHistory: () => apiClient.get<PlayerStatHistoryEntry[]>(API_ENDPOINTS.statsMine),
  getTeamTrend: () => apiClient.get<TeamTrendPoint[]>(API_ENDPOINTS.teamTrend),
}
