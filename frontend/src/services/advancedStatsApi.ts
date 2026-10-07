import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { MatchCategory } from './resultsApi'

/** One player's demo-derived totals across the counted matches; rating/ADR/KAST are null when no stat line had them. */
export interface AdvancedPlayer {
  userId: string
  name: string
  matches: number
  rounds: number
  openingWonT: number
  openingLostT: number
  openingWonCt: number
  openingLostCt: number
  tradeKills: number
  deaths: number
  tradedDeaths: number
  clutchAttempts: number
  clutchesWon: number
  bestClutchWon: number
  enemiesFlashed: number
  avgBlindSeconds: number
  teamFlashes: number
  utilityDamagePerMatch: number | null
  avgKast: number | null
  avgRating: number | null
  avgAdr: number | null
}

/** A player's averages on one map (one heatmap cell). */
export interface AdvancedMapCell {
  userId: string
  map: string
  matches: number
  avgRating: number
  avgAdr: number
}

/** A player's rating/ADR in one match, for the form chart. */
export interface AdvancedFormPoint {
  userId: string
  matchResultId: string
  playedAtUtc: string
  opponent: string
  map: string
  rating: number
  adr: number
}

/** Response of the advanced stats endpoint. */
export interface AdvancedStats {
  matchesAnalyzed: number
  matchesSkipped: number
  players: AdvancedPlayer[]
  mapCells: AdvancedMapCell[]
  form: AdvancedFormPoint[]
}

export const advancedStatsApi = {
  get: (category?: MatchCategory, map?: string, last?: number) =>
    apiClient.get<AdvancedStats>(API_ENDPOINTS.statsAdvanced(category, map, last)),
}
