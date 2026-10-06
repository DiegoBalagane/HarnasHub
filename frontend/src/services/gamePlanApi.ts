import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { MapSide } from './mapStrategyApi'
import type { MapName } from './nadesApi'
import type { EconomyType } from './tacticsApi'

export interface GamePlanTactic {
  id: string
  name: string
  mapName: MapName
  side: MapSide
  economy: EconomyType
}

export interface GamePlanBoard {
  id: string
  title: string
  mapName: MapName
}

/** An event's game plan: the coach's notes plus attached tactics and boards in the coach's order. */
export interface EventGamePlan {
  eventId: string
  notes: string | null
  tactics: GamePlanTactic[]
  boards: GamePlanBoard[]
  /** Null when no plan was written yet. */
  updatedAtUtc: string | null
}

export interface SetGamePlanPayload {
  notes: string | null
  tacticIds: string[]
  boardIds: string[]
}

export const gamePlanApi = {
  getPlan: (eventId: string) => apiClient.get<EventGamePlan>(API_ENDPOINTS.gamePlan(eventId)),
  /** Replaces the whole plan; list order is display order. */
  setPlan: (eventId: string, payload: SetGamePlanPayload) =>
    apiClient.put<EventGamePlan>(API_ENDPOINTS.gamePlan(eventId), payload),
}
