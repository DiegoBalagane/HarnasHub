import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { JobAccepted } from './jobsApi'
import type { MapName } from './nadesApi'
import type { MapTendencies } from './opponentDemosApi'
import type { VetoAction, VetoActor, VetoRecommendation } from './vetoApi'

export type InsightSeverity = 'High' | 'Warning' | 'Info'
export type ConfidenceLevel = 'Low' | 'Medium' | 'High'
export type OpponentVetoPrediction = 'Ban' | 'Pick' | 'Neutral' | 'Unknown'
export type VetoFormat = 'Bo1' | 'Bo3'

/** A cached FACEIT player. */
export interface FaceitPlayer {
  playerId: string
  nickname: string
  elo: number | null
  skillLevel: number | null
}

/** The FACEIT roster an opponent is linked to. */
export interface OpponentFaceitLink {
  opponentName: string
  faceitTeamId: string | null
  players: FaceitPlayer[]
  linkedAtUtc: string
  lastSyncedAtUtc: string | null
}

/** One TL;DR point with the numbers behind it. */
export interface OpponentInsight {
  kind: string
  severity: InsightSeverity
  text: string
  evidence: string
}

/** One row of the map matrix; rates, shares, trend and advantage are percentages / percentage points. */
export interface MapComparison {
  mapName: MapName
  theirGames: number
  theirWins: number
  theirWinRate: number | null
  theirAvgRoundDiff: number | null
  theirShare: number
  theirLastPlayedAtUtc: string | null
  theirTrend: number | null
  ourGames: number
  ourWins: number
  ourWinRate: number | null
  ourFaceitGames: number
  ourInternalGames: number
  poolStatus: string | null
  advantage: number
  confidence: ConfidenceLevel
  prediction: OpponentVetoPrediction
  predictionReason: string
  vetoScore: number
  recommendation: VetoRecommendation
  vetoReasons: string[]
}

/** One step of a simulated veto. */
export interface VetoPlanStep {
  order: number
  actor: VetoActor
  action: VetoAction
  mapName: MapName
  reason: string
}

export interface VetoPlan {
  format: VetoFormat
  steps: VetoPlanStep[]
}

export interface PlayerToWatch {
  playerId: string
  nickname: string
  games: number
  kdRatio: number
  adr: number | null
  headshotPercent: number | null
  multiKillsPerGame: number
}

export interface MapPlayersToWatch {
  mapName: MapName
  players: PlayerToWatch[]
}

export interface FormGame {
  faceitMatchId: string
  playedAtUtc: string
  mapName: string | null
  roundsFor: number
  roundsAgainst: number
  won: boolean
  competitionName: string | null
}

export interface OpponentForm {
  lastGames: FormGame[]
  /** e.g. "W3" or "L2"; null without games. */
  streak: string | null
  newPlayers: string[]
}

/** The FACEIT "them vs us" report of one opponent. */
export interface OpponentReport {
  opponentName: string
  faceitConfigured: boolean
  link: OpponentFaceitLink | null
  generatedAtUtc: string
  dataSyncedAtUtc: string | null
  theirTeamGames: number
  theirSoloGames: number
  ourTeamGames: number
  ourInternalGames: number
  ourLinkedPlayers: number
  insights: OpponentInsight[]
  maps: MapComparison[]
  vetoPlans: VetoPlan[]
  playersToWatch: MapPlayersToWatch[]
  form: OpponentForm
  nextEventId: string | null
  nextEventAtUtc: string | null
  /** Tendencies from the opponent's analysed demos per map; empty without demos. */
  tendencies: MapTendencies[]
}

export const opponentReportApi = {
  getReport: (name: string) => apiClient.get<OpponentReport>(API_ENDPOINTS.opponents.report(name)),
  /** Links the opponent from a FACEIT team URL, match room URL or nickname list. */
  /** Starts a link-and-sync job (result: OpponentFaceitLink). */
  link: (opponentName: string, source: string) =>
    apiClient.put<JobAccepted>(API_ENDPOINTS.opponents.reportLink, { opponentName, source }),
  /** Pulls fresh FACEIT data and regenerates the report (rate-limited on the server). */
  /** Starts a manual FACEIT refresh job (result: OpponentReport). */
  refresh: (name: string) => apiClient.post<JobAccepted>(API_ENDPOINTS.opponents.reportRefresh(name), undefined),
}
