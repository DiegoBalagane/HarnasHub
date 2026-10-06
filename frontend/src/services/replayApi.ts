import { API_ENDPOINTS } from '../constants'
import type { AnalysisBoard } from './analysisBoardsApi'
import { apiClient } from './apiClient'
import type { BombSite, RoundEndReason, Side } from './matchAnalysisApi'
import type { DemoGrenadeType } from './opponentDemosApi'

/** Whether positions can be drawn: available, not recorded (old timeline) or the map has no verified radar fit. */
export type ReplayPositionsStatus = 'Available' | 'NotRecorded' | 'UncalibratedMap'
export type ReplayTeam = 'Unknown' | 'Ours' | 'Opponent'
/** Which stored timeline a replayed round comes from. */
export type ReplaySource = 'Match' | 'OpponentDemo'

/** A round participant; `index` is what frames, kills and grenades refer to. */
export interface ReplayPlayer {
  index: number
  steamId64: string
  name: string
  side: Side
  team: ReplayTeam
}

/** One alive player's state in a frame (radar fractions in [0,1]). */
export interface ReplayPlayerState {
  player: number
  x: number
  y: number
  health: number
  weapon: string | null
}

/** Everyone alive at one whole second; `frames[s].second === s`. */
export interface ReplayFrame {
  second: number
  players: ReplayPlayerState[]
}

/** A kill; x/y is where the victim died (null without positions). */
export interface ReplayKill {
  second: number
  killer: number | null
  victim: number
  weapon: string
  headshot: boolean
  isTeamKill: boolean
  x: number | null
  y: number | null
}

/** One grenade from throw to the end of its effect (`timesApproximate` = nominal fuse used). */
export interface ReplayGrenade {
  id: number
  type: DemoGrenadeType
  thrower: number | null
  side: Side | null
  throwSecond: number
  detonateSecond: number
  endSecond: number
  timesApproximate: boolean
  throwX: number | null
  throwY: number | null
  landX: number | null
  landY: number | null
}

/** Bomb plant with its defuse or explosion. */
export interface ReplayBomb {
  plantSecond: number
  site: BombSite | null
  planter: number | null
  x: number | null
  y: number | null
  defuseSecond: number | null
  defuser: number | null
  explodeSecond: number | null
}

/** A compact 2D replay of one round. */
export interface RoundReplay {
  mapName: string | null
  roundNumber: number
  roundsCount: number
  opponentName: string | null
  positionsStatus: ReplayPositionsStatus
  durationSeconds: number
  winnerSide: Side | null
  endReason: RoundEndReason
  ourSide: Side | null
  players: ReplayPlayer[]
  frames: ReplayFrame[]
  kills: ReplayKill[]
  grenades: ReplayGrenade[]
  bomb: ReplayBomb | null
}

export const replayApi = {
  getMatchRound: (matchResultId: string, roundNumber: number) =>
    apiClient.get<RoundReplay>(API_ENDPOINTS.roundReplay(matchResultId, roundNumber)),
  getOpponentDemoRound: (demoId: string, roundNumber: number) =>
    apiClient.get<RoundReplay>(API_ENDPOINTS.opponents.demoReplay(demoId, roundNumber)),
  /** Saves the round at `second` as an analysis board drawn server-side. */
  createBoardFromRound: (payload: {
    source: ReplaySource
    sourceId: string
    roundNumber: number
    second: number
  }) => apiClient.post<AnalysisBoard>(API_ENDPOINTS.analysisBoards.fromRound, payload),
}
