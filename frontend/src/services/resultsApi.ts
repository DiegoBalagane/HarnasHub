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

/** One demo participant's raw totals, as returned by analyzeDemo — round-tripped back on addResult so a stat line
 * can be saved without re-uploading the (possibly 100-300MB) demo file a second time. */
export interface AnalyzedDemoPlayer {
  steamId64: string
  demoPlayerName: string
  kills: number
  deaths: number
  assists: number
  headshots: number
  damageDealt: number
  entryKills: number
  entryDeaths: number
  kastRounds: number
  utilityDamage: number
  flashAssists: number
  multiKill2K: number
  multiKill3K: number
  multiKill4K: number
  multiKill5K: number
}

/** One of the two groups a demo's round-1 sides split into, with the score it would produce if this is "our" team. */
export interface DemoTeamPreview {
  playerNames: string[]
  ourScore: number
  opponentScore: number
}

export interface AnalyzeDemoResult {
  roundsPlayed: number
  mapName: string | null
  teamA: DemoTeamPreview
  teamB: DemoTeamPreview
  /** 'A' | 'B' when the roster's SteamID64s overlap one of the two groups — null when nobody matched. */
  suggestedTeam: 'A' | 'B' | null
  players: AnalyzedDemoPlayer[]
}

export interface AddResultPayload {
  opponent: string
  ourScore?: number
  opponentScore?: number
  mapName?: string
  /** An external link to the demo for download — a separate, optional thing from the file analyzeDemo already parsed. */
  demoUrl?: string
  notes?: string
  playedAtUtc: string
  category: MatchCategory
  tournamentId?: string | null
  leagueId?: string | null
  /** Present only when the coach analysed a demo first — lets the server import a stat line per matched roster member. */
  demoRoundsPlayed?: number
  demoPlayers?: AnalyzedDemoPlayer[]
}

export const resultsApi = {
  getResults: () => apiClient.get<MatchResult[]>(API_ENDPOINTS.results),
  addResult: (payload: AddResultPayload) => apiClient.post<MatchResult>(API_ENDPOINTS.results, payload),
  analyzeDemo: (demoFile: File) => {
    const formData = new FormData()
    formData.append('demo', demoFile)
    return apiClient.postForm<AnalyzeDemoResult>(API_ENDPOINTS.analyzeResultDemo, formData)
  },
}
