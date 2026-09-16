import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { MapName } from './nadesApi'
import type { TeamRole } from './rosterApi'

/** Side of the map a starting-position setup applies to, mirrors the backend MapSide enum. */
export type MapSide = 'CT' | 'T'

export interface MapPosition {
  id: string
  userId: string
  displayName: string
  inGameNickname: string | null
  teamRole: TeamRole | null
  label: string | null
  /** Radar-relative fraction in [0,1], measured from the left edge. */
  x: number
  /** Radar-relative fraction in [0,1], measured from the top edge. */
  y: number
  note: string | null
}

export interface SetPlayerPositionPayload {
  mapName: MapName
  side: MapSide
  userId: string
  label?: string | null
  x: number
  y: number
  note?: string | null
}

export const mapStrategyApi = {
  getPositions: (mapName: MapName, side: MapSide) =>
    apiClient.get<MapPosition[]>(API_ENDPOINTS.mapStrategy.positions(mapName, side)),
  setPosition: (payload: SetPlayerPositionPayload) =>
    apiClient.post<MapPosition>(API_ENDPOINTS.mapStrategy.set, payload),
  removePosition: (positionId: string) =>
    apiClient.delete<void>(API_ENDPOINTS.mapStrategy.remove(positionId)),
}
