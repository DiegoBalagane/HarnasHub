import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { MapName } from './nadesApi'

export type VetoActor = 'Us' | 'Opponent'
export type VetoAction = 'Ban' | 'Pick' | 'Decider'

/** One step of a veto as entered in the editor; its position in the list is its order. */
export interface VetoStepInput {
  actor: VetoActor
  action: VetoAction
  mapName: MapName
}

/** One recorded veto step, 1-based order. */
export interface VetoStep extends VetoStepInput {
  order: number
}

export type VetoRecommendation = 'Pick' | 'Ban' | 'Neutral'

/** A map's place in the suggested veto, with the reasons behind its score. */
export interface MapVetoSuggestion {
  mapName: MapName
  score: number
  recommendation: VetoRecommendation
  reasons: string[]
}

export interface OpponentMapTendency {
  mapName: MapName
  picks: number
  bans: number
}

/** Suggested veto against one opponent: best pick first, first ban last. */
export interface VetoSuggestion {
  opponentName: string
  maps: MapVetoSuggestion[]
  opponentTendencies: OpponentMapTendency[]
  /** How many recorded vetoes against this opponent the tendencies are based on. */
  recordedOpponentVetoes: number
}

export const vetoApi = {
  getSuggestion: (opponent: string) => apiClient.get<VetoSuggestion>(API_ENDPOINTS.veto.suggestion(opponent)),
  getEventVeto: (eventId: string) => apiClient.get<VetoStep[]>(API_ENDPOINTS.veto.event(eventId)),
  /** Replaces the whole veto of the event; an empty list clears it. */
  setEventVeto: (eventId: string, steps: VetoStepInput[]) =>
    apiClient.put<VetoStep[]>(API_ENDPOINTS.veto.event(eventId), { steps }),
}
