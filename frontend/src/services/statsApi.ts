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

export interface ParsedPlayerStat {
  /** String, not a number — a SteamID64 exceeds Number.MAX_SAFE_INTEGER. */
  steamId64: string
  demoPlayerName: string
  /** Set only when the demo participant's SteamID64 matches a roster member's own (see Ustawienia). */
  matchedUserId: string | null
  matchedDisplayName: string | null
  kills: number
  deaths: number
  assists: number
  adr: number
  headshotPercentage: number
  /** A rough approximation (kills/deaths/assists per round plus a damage term) — always worth reviewing before saving. */
  rating: number
}

export interface ImportStatsFromDemoResult {
  roundsPlayed: number
  players: ParsedPlayerStat[]
}

export const statsApi = {
  getMatchStats: (matchResultId: string) =>
    apiClient.get<PlayerMatchStat[]>(API_ENDPOINTS.matchStats(matchResultId)),
  addPlayerStat: (matchResultId: string, payload: AddPlayerStatPayload) =>
    apiClient.post<PlayerMatchStat>(API_ENDPOINTS.matchStats(matchResultId), payload),
  getMyStatsHistory: () => apiClient.get<PlayerStatHistoryEntry[]>(API_ENDPOINTS.statsMine),
  getTeamTrend: () => apiClient.get<TeamTrendPoint[]>(API_ENDPOINTS.teamTrend),
  importStatsFromDemo: (demoFile: File) => {
    const formData = new FormData()
    formData.append('demo', demoFile)
    return apiClient.postForm<ImportStatsFromDemoResult>(API_ENDPOINTS.importStatsFromDemo, formData)
  },
}
