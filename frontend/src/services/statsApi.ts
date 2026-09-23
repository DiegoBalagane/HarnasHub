import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { MatchCategory } from './resultsApi'

export type MapSide = 'CT' | 'T'

/** A death location as a radar-relative fraction in [0,1] — same convention as map-strategy/nades pins. */
export interface DeathPosition {
  x: number
  y: number
  side: MapSide
}

/** Everything from entryKills onward is only ever set on a row that came from a demo import — null on a manually entered row.
 * userId is null for a demo-imported player nobody on the roster has claimed with a matching SteamID64 yet. */
export interface PlayerMatchStat {
  id: string
  userId: string | null
  displayName: string
  kills: number
  deaths: number
  assists: number
  adr: number
  headshotPercentage: number
  rating: number
  entryKills: number | null
  entryDeaths: number | null
  kastPercentage: number | null
  multiKill2K: number | null
  multiKill3K: number | null
  multiKill4K: number | null
  multiKill5K: number | null
  utilityDamage: number | null
  flashAssists: number | null
  deathPositions: DeathPosition[]
}

export interface AddPlayerStatPayload {
  userId: string
  kills: number
  deaths: number
  assists: number
  adr: number
  headshotPercentage: number
  rating: number
  entryKills?: number
  entryDeaths?: number
  kastPercentage?: number
  multiKill2K?: number
  multiKill3K?: number
  multiKill4K?: number
  multiKill5K?: number
  utilityDamage?: number
  flashAssists?: number
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

/** One player's stats averaged across every match counted in the current filter — for comparing the team against itself. */
export interface PlayerLeaderboardEntry {
  userId: string
  displayName: string
  inGameNickname: string | null
  matchesPlayed: number
  avgKills: number
  avgDeaths: number
  avgAssists: number
  avgAdr: number
  avgRating: number
  avgHeadshotPercentage: number
  avgKastPercentage: number | null
  avgEntryKills: number | null
  avgEntryDeaths: number | null
  avgUtilityDamage: number | null
  avgFlashAssists: number | null
}

export const statsApi = {
  getMatchStats: (matchResultId: string) =>
    apiClient.get<PlayerMatchStat[]>(API_ENDPOINTS.matchStats(matchResultId)),
  addPlayerStat: (matchResultId: string, payload: AddPlayerStatPayload) =>
    apiClient.post<PlayerMatchStat>(API_ENDPOINTS.matchStats(matchResultId), payload),
  getMyStatsHistory: () => apiClient.get<PlayerStatHistoryEntry[]>(API_ENDPOINTS.statsMine),
  getTeamTrend: () => apiClient.get<TeamTrendPoint[]>(API_ENDPOINTS.teamTrend),
  getLeaderboard: (category?: MatchCategory) =>
    apiClient.get<PlayerLeaderboardEntry[]>(API_ENDPOINTS.statsLeaderboard(category)),
}
