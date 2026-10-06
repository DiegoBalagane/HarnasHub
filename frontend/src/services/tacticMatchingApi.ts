import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { MapSide } from './mapStrategyApi'
import type { MapName } from './nadesApi'

/** The Playbook tactic one round was automatically matched to, with its score 0–100. */
export interface RoundTacticMatch {
  roundNumber: number
  side: MapSide
  tacticId: string
  tacticName: string
  scorePercent: number
}

/** Per-round tactic matches of one match; flags explain an empty result. */
export interface MatchTacticMatches {
  mapCalibrated: boolean
  hasPositions: boolean
  ourTeamResolved: boolean
  rounds: RoundTacticMatch[]
}

/** One tactic's record over the newest analysed matches on its map. */
export interface TacticEffectiveness {
  tacticId: string
  name: string
  side: MapSide
  roundsPlayed: number
  roundsWon: number
  matches: number
}

/** Effectiveness of every tactic of a map. */
export interface TacticEffectivenessReport {
  map: MapName
  mapCalibrated: boolean
  matchesAnalyzed: number
  matchesSkipped: number
  roundsAnalyzed: number
  roundsMatched: number
  tactics: TacticEffectiveness[]
}

export const tacticMatchingApi = {
  getMatchTacticMatches: (matchResultId: string) =>
    apiClient.get<MatchTacticMatches>(API_ENDPOINTS.matchTacticMatches(matchResultId)),
  getEffectiveness: (mapName: MapName) =>
    apiClient.get<TacticEffectivenessReport>(API_ENDPOINTS.tactics.effectiveness(mapName)),
}
