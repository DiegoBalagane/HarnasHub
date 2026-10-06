import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { MapName } from './nadesApi'
import type { MatchCategory } from './resultsApi'

/** Where a map stands in the team's pool — mirrors the backend MapPoolStatus enum. */
export type MapPoolStatus = 'Core' | 'Playable' | 'Learning' | 'Ban'

/** One map of the pool: the coach's classification plus the team's record on it. */
export interface MapPoolMap {
  mapName: MapName
  /** Null when the map hasn't been classified yet. */
  status: MapPoolStatus | null
  note: string | null
  wins: number
  losses: number
  draws: number
  /** Null when nothing was played on the map. */
  winRatePercentage: number | null
  /** Up to the last five outcomes, newest first. */
  recentForm: ('W' | 'L' | 'D')[]
  lastPlayedAtUtc: string | null
  tacticCount: number
}

export interface SetMapPoolEntryPayload {
  /** Null clears the map's classification. */
  status: MapPoolStatus | null
  note?: string
}

export const mapPoolApi = {
  getMapPool: (category?: MatchCategory) => apiClient.get<MapPoolMap[]>(API_ENDPOINTS.mapPool.list(category)),
  setEntry: (mapName: MapName, payload: SetMapPoolEntryPayload) =>
    apiClient.put<void>(API_ENDPOINTS.mapPool.byMap(mapName), payload),
}
