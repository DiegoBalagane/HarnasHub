import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { MapName } from './nadesApi'
import type { PinColor, TeamRole } from './rosterApi'

/** Side of the map a starting-position setup applies to, mirrors the backend MapSide enum. */
export type MapSide = 'CT' | 'T'

export interface MapPosition {
  id: string
  userId: string
  displayName: string
  inGameNickname: string | null
  teamRole: TeamRole | null
  /** Self-chosen colour (Main roster only); falls back to an automatic per-user colour when unset. */
  pinColor: PinColor | null
  /** Self-chosen single character shown on the pin; falls back to auto-generated initials when unset. */
  pinMark: string | null
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

/** A free-floating text label on the radar — not tied to a player, e.g. a callout note. */
export interface MapTextAnnotation {
  id: string
  text: string
  /** Hex color, e.g. "#ffffff". */
  color: string
  fontSizePx: number
  x: number
  y: number
}

export interface AddTextAnnotationPayload {
  mapName: MapName
  side: MapSide
  text: string
  color: string
  fontSizePx: number
  x: number
  y: number
}

export interface UpdateTextAnnotationPayload {
  annotationId: string
  text: string
  color: string
  fontSizePx: number
  x: number
  y: number
}

export const mapStrategyApi = {
  getPositions: (mapName: MapName, side: MapSide) =>
    apiClient.get<MapPosition[]>(API_ENDPOINTS.mapStrategy.positions(mapName, side)),
  setPosition: (payload: SetPlayerPositionPayload) =>
    apiClient.post<MapPosition>(API_ENDPOINTS.mapStrategy.set, payload),
  removePosition: (positionId: string) =>
    apiClient.delete<void>(API_ENDPOINTS.mapStrategy.remove(positionId)),
  getTextAnnotations: (mapName: MapName, side: MapSide) =>
    apiClient.get<MapTextAnnotation[]>(API_ENDPOINTS.mapStrategy.textAnnotations(mapName, side)),
  addTextAnnotation: (payload: AddTextAnnotationPayload) =>
    apiClient.post<MapTextAnnotation>(API_ENDPOINTS.mapStrategy.addTextAnnotation, payload),
  updateTextAnnotation: ({ annotationId, ...payload }: UpdateTextAnnotationPayload) =>
    apiClient.patch<MapTextAnnotation>(API_ENDPOINTS.mapStrategy.textAnnotationById(annotationId), payload),
  removeTextAnnotation: (annotationId: string) =>
    apiClient.delete<void>(API_ENDPOINTS.mapStrategy.textAnnotationById(annotationId)),
}
