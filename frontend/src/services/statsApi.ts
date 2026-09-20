import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

/** Everything from entryKills onward is only ever set on a row that came from a demo import — null on a manually entered row. */
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
  entryKills: number | null
  entryDeaths: number | null
  kastPercentage: number | null
  multiKill2K: number | null
  multiKill3K: number | null
  multiKill4K: number | null
  multiKill5K: number | null
  utilityDamage: number | null
  flashAssists: number | null
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

export type MapSide = 'CT' | 'T'

/** A death location as a radar-relative fraction in [0,1] — same convention as map-strategy/nades pins. */
export interface DeathPosition {
  x: number
  y: number
  side: MapSide
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
  /** A rough approximation (kills/deaths/assists per round, a damage term, and KAST%) — always worth reviewing before saving. */
  rating: number
  entryKills: number
  entryDeaths: number
  kastPercentage: number
  multiKill2K: number
  multiKill3K: number
  multiKill4K: number
  multiKill5K: number
  utilityDamage: number
  flashAssists: number
  deathPositions: DeathPosition[]
}

export interface ImportStatsFromDemoResult {
  roundsPlayed: number
  /** Null when the demo's map isn't in the current pool — the death-map view has nothing to draw on in that case. */
  mapName: string | null
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
