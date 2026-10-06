import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { JobAccepted } from './jobsApi'
import type { GrenadeType } from './nadesApi'

export type Side = 'T' | 'CT'
export type BuyType = 'Pistol' | 'Eco' | 'SemiEco' | 'Force' | 'Full'
export type RoundEndReason = 'Other' | 'BombExploded' | 'BombDefused' | 'Elimination' | 'TimeExpired' | 'Surrender'
export type BombSite = 'A' | 'B'
export type InsightTone = 'Neutral' | 'Positive' | 'Negative'

/** One team's buy in a round. */
export interface TeamEconomy {
  side: Side
  equipmentValue: number
  money: number
  players: number
  buyType: BuyType
}

/** Bomb plant (and defuse) of a round. */
export interface RoundBomb {
  site: BombSite | null
  plantSecondsIntoRound: number
  planterName: string | null
  defused: boolean
  defuseSecondsIntoRound: number | null
  defuserName: string | null
}

/** One kill within a round; byUs is null for world/team kills or when our side is unknown. */
export interface RoundKill {
  secondsIntoRound: number
  killerName: string | null
  killerSteamId64: string | null
  killerSide: Side | null
  victimName: string
  victimSteamId64: string
  victimSide: Side | null
  assisterName: string | null
  weapon: string
  headshot: boolean
  wallbang: boolean
  throughSmoke: boolean
  noScope: boolean
  attackerBlind: boolean
  isOpening: boolean
  isTeamKill: boolean
  byUs: boolean | null
  killerZone: string | null
  victimZone: string | null
}

/** One round of the match timeline. */
export interface MatchRound {
  number: number
  winnerSide: Side | null
  ourSide: Side | null
  weWon: boolean | null
  ourScoreAfter: number
  opponentScoreAfter: number
  endReason: RoundEndReason
  durationSeconds: number | null
  ourEconomy: TeamEconomy | null
  opponentEconomy: TeamEconomy | null
  bomb: RoundBomb | null
  kills: RoundKill[]
}

/** A match's round-by-round timeline; when ourTeamResolved is false "our" means the team that started on T. */
export interface MatchTimeline {
  mapName: string | null
  hasZones: boolean
  ourTeamResolved: boolean
  parserVersion: number
  rounds: MatchRound[]
}

/** One automatic, rule-based observation about the match. */
export interface MatchInsight {
  code: string
  tone: InsightTone
  title: string
  detail: string
  sampleSize: number
}

/** Result of attaching a demo to an existing match. */
export interface MatchDemoAnalysis {
  matchResultId: string
  roundsCount: number
  parserVersion: number
  ourTeamResolved: boolean
}

/** One 1vX situation. */
export interface Clutch {
  roundNumber: number
  steamId64: string
  name: string
  versus: number
  won: boolean
  isOurs: boolean
}

/** Trade numbers of one of our players. */
export interface TradePlayer {
  steamId64: string
  name: string
  deaths: number
  tradedDeaths: number
  tradeKills: number
}

/** Team trade totals (deaths to enemies only) plus our players. */
export interface TradeSummary {
  ourDeaths: number
  ourTradedDeaths: number
  ourTradeKills: number
  opponentDeaths: number
  opponentTradedDeaths: number
  players: TradePlayer[]
}

/** Clutch record of one of our players. */
export interface ClutchPlayer {
  steamId64: string
  name: string
  attempts: number
  won: number
  bestWonVersus: number
}

/** Clutch totals for both teams, our players and every clutch. */
export interface ClutchSummary {
  ourAttempts: number
  ourWon: number
  opponentAttempts: number
  opponentWon: number
  players: ClutchPlayer[]
  clutches: Clutch[]
}

/** One opening duel we took part in; coordinates are radar fractions. */
export interface OpeningDuel {
  roundNumber: number
  ourSide: Side | null
  wonByUs: boolean
  zone: string | null
  killerName: string | null
  victimName: string
  killerX: number | null
  killerY: number | null
  victimX: number | null
  victimY: number | null
}

/** Opening duels won/lost on one side in one zone. */
export interface OpeningBucket {
  side: Side
  zone: string | null
  won: number
  lost: number
}

/** Opening duel record of one of our players. */
export interface OpeningPlayer {
  steamId64: string
  name: string
  won: number
  lost: number
}

/** Opening duels of a match with rollups. */
export interface OpeningSummary {
  duels: OpeningDuel[]
  buckets: OpeningBucket[]
  players: OpeningPlayer[]
}

/** Flash effectiveness of one of our players. */
export interface FlashPlayer {
  steamId64: string
  name: string
  enemiesFlashed: number
  avgEnemyBlindSeconds: number
  teamFlashes: number
  teamBlindSeconds: number
}

/** Flash stats; hasData is false for timelines parsed before blind tracking existed. */
export interface FlashSummary {
  hasData: boolean
  enemiesFlashed: number
  teamFlashes: number
  players: FlashPlayer[]
}

/** A pinned library nade and how often it was thrown in the match. */
export interface LibraryNadeUsage {
  id: string
  type: GrenadeType
  title: string
  landingX: number
  landingY: number
  timesThrown: number
}

/** A grenade spot thrown repeatedly that the library lacks. */
export interface UnlistedGrenade {
  type: GrenadeType
  landingX: number
  landingY: number
  throwX: number | null
  throwY: number | null
  count: number
  zone: string | null
}

/** Per-type coverage of the library. */
export interface GrenadeTypeCoverage {
  type: GrenadeType
  total: number
  thrown: number
}

/** Match grenades vs the team nade library. */
export interface GrenadeLibraryComparison {
  trainedTotal: number
  trainedThrown: number
  ourGrenadesThrown: number
  byType: GrenadeTypeCoverage[]
  library: LibraryNadeUsage[]
  candidates: UnlistedGrenade[]
}

/** Deep single-match analysis. */
export interface MatchAnalysis {
  mapName: string | null
  hasZones: boolean
  ourTeamResolved: boolean
  trades: TradeSummary
  clutches: ClutchSummary
  openings: OpeningSummary
  flashes: FlashSummary
  grenades: GrenadeLibraryComparison | null
}

/** Rounds won out of rounds played. */
export interface WinRate {
  won: number
  total: number
}

/** Win rate per buy type. */
export interface BuyTypeWinRate {
  buyType: BuyType
  won: number
  total: number
}

/** Bombsite stats; site null = unknown. */
export interface SiteStat {
  site: BombSite | null
  won: number
  total: number
}

/** Opening duels per side and zone summed over matches. */
export interface MapOpeningZone {
  side: Side
  zone: string | null
  won: number
  lost: number
}

/** A player clutch record over matches. */
export interface MapClutcher {
  steamId64: string
  name: string
  attempts: number
  won: number
}

/** Multi-match aggregates of one map for the Playbook statistics tab. */
export interface MapAnalytics {
  map: string
  hasZones: boolean
  matchesAnalyzed: number
  matchesSkipped: number
  roundsAnalyzed: number
  tSide: WinRate
  ctSide: WinRate
  pistol: WinRate
  buyTypes: BuyTypeWinRate[]
  tSites: SiteStat[]
  ctRetakes: SiteStat[]
  openings: MapOpeningZone[]
  trades: { ourDeaths: number; ourTradedDeaths: number; ourTradeKills: number }
  topClutchers: MapClutcher[]
  insights: MatchInsight[]
}

export const matchAnalysisApi = {
  getTimeline: (matchResultId: string) => apiClient.get<MatchTimeline>(API_ENDPOINTS.matchTimeline(matchResultId)),
  getInsights: (matchResultId: string) => apiClient.get<MatchInsight[]>(API_ENDPOINTS.matchInsights(matchResultId)),
  getAnalysis: (matchResultId: string) => apiClient.get<MatchAnalysis>(API_ENDPOINTS.matchAnalysis(matchResultId)),
  getMapAnalytics: (mapName: string) => apiClient.get<MapAnalytics>(API_ENDPOINTS.mapAnalytics(mapName)),
  /** Starts a background job (result: MatchDemoAnalysis); objectKey comes from resultsApi.presignDemoUpload after the PUT. */
  attachDemo: (matchResultId: string, objectKey: string) =>
    apiClient.post<JobAccepted>(API_ENDPOINTS.attachResultDemo(matchResultId), { objectKey }),
}
