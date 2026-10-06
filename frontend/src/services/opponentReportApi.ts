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
  /** Our win rate smoothed towards 50%; absent in older reports. */
  ourSmoothedWinRate?: number | null
  /** Their recency-weighted, smoothed win rate the decisions use; absent in older reports. */
  theirSmoothedWinRate?: number | null
  /** True when our sample is too small to decide anything ("za mało danych"). */
  ourLowSample?: boolean
  /** True when their sample on the map is too small to quote a win rate. */
  theirLowSample?: boolean
  /** Lifetime FACEIT numbers of their active lineup on the map. */
  theirLifetime?: MapLifetime | null
  /** Lifetime FACEIT numbers of our linked players on the map. */
  ourLifetime?: MapLifetime | null
}

/** A lineup's summed lifetime FACEIT numbers on one map (like the FACEIT match room); rates and share in percent. */
export interface MapLifetime {
  players: number
  matches: number
  winRate: number | null
  avgKdRatio: number | null
  share: number
  /** The lineup plays this map a lot individually. */
  experienced: boolean
}

/** One linked opponent player in the lineup header. */
export interface LineupPlayer {
  playerId: string
  nickname: string
  elo: number | null
  skillLevel: number | null
  recentTeamGames: number
  teamGames: number
  lastTeamGameAtUtc: string | null
}

/** A roster player whose FACEIT account could not be found, with the Polish reason. */
export interface UnresolvedRosterPlayer {
  userId: string
  displayName: string
  reason: string
}

/** Who the report treats as the active lineup and why; inactive = ex-members and subs left out of player numbers. */
export interface ActiveLineup {
  basis: string
  windowGames: number
  active: LineupPlayer[]
  inactive: LineupPlayer[]
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

export type FormDirection = 'Up' | 'Down' | 'Flat'

/** One player's numbers on one map; rates and share in percent, team/solo split by "≥ 3 linked players on one side". */
export interface PlayerMapForm {
  mapName: MapName
  games: number
  share: number
  wins: number
  winRate: number
  kdRatio: number
  adr: number | null
  headshotPercent: number | null
  lastPlayedAtUtc: string
  teamGames: number
  soloGames: number
  teamWinRate: number | null
  soloWinRate: number | null
  teamKdRatio: number | null
  soloKdRatio: number | null
}

/** Last 10 games vs the earlier ones; win rates in percent, the delta in percentage points. */
export interface PlayerRecentForm {
  recentGames: number
  earlierGames: number
  recentKdRatio: number
  earlierKdRatio: number
  recentWinRate: number
  earlierWinRate: number
  kdDelta: number
  winRateDelta: number
  direction: FormDirection
}

/** One linked player's individual form across team and solo games. */
export interface PlayerForm {
  playerId: string
  nickname: string
  elo: number | null
  skillLevel: number | null
  games: number
  teamGames: number
  soloGames: number
  winRate: number | null
  kdRatio: number | null
  adr: number | null
  headshotPercent: number | null
  lastPlayedAtUtc: string | null
  recentForm: PlayerRecentForm | null
  maps: PlayerMapForm[]
}

/** A roster's comfort on a map from solo games: of the rated players, how many play it regularly / avoid it. */
export interface MapComfort {
  mapName: MapName
  ratedPlayers: number
  regularPlayers: number
  avoidingPlayers: number
  soloShare: number
  avgWinRate: number | null
  avgKdRatio: number | null
  regularNicknames: string[]
  avoidingNicknames: string[]
}

export interface TeamIndividualForm {
  players: PlayerForm[]
  mapComfort: MapComfort[]
}

/** Individual form of both rosters. */
export interface IndividualForm {
  theirs: TeamIndividualForm
  ours: TeamIndividualForm
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
  /** Individual form (team + solo games) of both rosters; null/absent in reports generated before it existed. */
  individualForm?: IndividualForm | null
  /** The opponent's active lineup; null/absent in reports generated before it existed. */
  activeLineup?: ActiveLineup | null
  /** Roster players without a FACEIT account found, with the reason; absent in older payloads. */
  unresolvedOurPlayers?: UnresolvedRosterPlayer[] | null
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
