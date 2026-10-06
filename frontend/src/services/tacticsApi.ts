import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { JobAccepted } from './jobsApi'
import type { GrenadeType, MapName } from './nadesApi'
import type { MapSide } from './mapStrategyApi'

/** Round economy a tactic is designed for, mirrors the backend EconomyType enum. */
export type EconomyType = 'Eco' | 'ForceBuy' | 'FullBuy' | 'AntiEco'

export interface TacticPoint {
  id: string
  order: number
  /** Radar-relative fraction in [0,1], measured from the left edge. */
  x: number
  /** Radar-relative fraction in [0,1], measured from the top edge. */
  y: number
  description: string | null
  nadeEntryId: string | null
}

export interface Tactic {
  id: string
  mapName: MapName
  side: MapSide
  name: string
  economy: EconomyType
  note: string | null
  createdByUserId: string
  pointCount: number
}

export interface TacticDetail {
  id: string
  mapName: MapName
  side: MapSide
  name: string
  economy: EconomyType
  note: string | null
  createdByUserId: string
  points: TacticPoint[]
}

export interface TacticPointInput {
  x: number
  y: number
  description?: string | null
  nadeEntryId?: string | null
}

export interface CreateTacticPayload {
  mapName: MapName
  side: MapSide
  name: string
  economy: EconomyType
  note?: string | null
}

export interface UpdateTacticPayload {
  name: string
  economy: EconomyType
  note?: string | null
  points: TacticPointInput[]
}

/** One grenade thrown in a demo round; positions are radar fractions in [0,1]. */
export interface DemoNade {
  id: number
  type: GrenadeType
  throwerName: string
  /** SteamID64 as a string — it exceeds JavaScript's safe-integer range. */
  throwerSteamId: string | null
  side: MapSide
  throwX: number
  throwY: number
  landX: number
  landY: number
  secondsIntoRound: number
}

/** One demo round with the score after it (per side) and its grenades. */
export interface DemoNadeRound {
  number: number
  winnerSide: MapSide | null
  terroristScore: number
  counterTerroristScore: number
  grenades: DemoNade[]
}

/** Every round of a parsed demo, returned by the extract step of the import wizard. */
export interface DemoNades {
  mapName: MapName
  rounds: DemoNadeRound[]
}

/** One grenade selected for import. */
export interface ImportedNadeInput {
  type: GrenadeType
  throwerName: string
  throwX: number
  throwY: number
  landX: number
  landY: number
  secondsIntoRound: number
}

export interface ImportTacticFromDemoPayload {
  mapName: MapName
  side: MapSide
  name: string
  economy: EconomyType
  note?: string | null
  grenades: ImportedNadeInput[]
  addToNadeLibrary: boolean
}

export const tacticsApi = {
  /** Parses a demo already uploaded through the presigned demo upload; the server deletes the object afterwards. */
  /** Starts a background job (result: DemoNades) parsing a presigned-uploaded demo. */
  extractDemoNades: (objectKey: string) => apiClient.post<JobAccepted>(API_ENDPOINTS.tactics.importDemoExtract, { objectKey }),
  importFromDemo: (payload: ImportTacticFromDemoPayload) =>
    apiClient.post<TacticDetail>(API_ENDPOINTS.tactics.importDemo, payload),
  getTactics: (filters: { mapName?: MapName; side?: MapSide; economy?: EconomyType }) => {
    const params = new URLSearchParams()
    if (filters.mapName) params.set('mapName', filters.mapName)
    if (filters.side) params.set('side', filters.side)
    if (filters.economy) params.set('economy', filters.economy)
    const query = params.toString()
    return apiClient.get<Tactic[]>(`${API_ENDPOINTS.tactics.list}${query ? `?${query}` : ''}`)
  },
  getTacticDetail: (tacticId: string) => apiClient.get<TacticDetail>(API_ENDPOINTS.tactics.byId(tacticId)),
  createTactic: (payload: CreateTacticPayload) => apiClient.post<TacticDetail>(API_ENDPOINTS.tactics.list, payload),
  updateTactic: (tacticId: string, payload: UpdateTacticPayload) =>
    apiClient.put<TacticDetail>(API_ENDPOINTS.tactics.byId(tacticId), payload),
  deleteTactic: (tacticId: string) => apiClient.delete<void>(API_ENDPOINTS.tactics.byId(tacticId)),
}
