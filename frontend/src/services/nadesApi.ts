import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export type GrenadeType = 'Smoke' | 'Flash' | 'Molotov' | 'Frag'

/** Maps of the current competitive pool, mirrors the backend MapName enum. */
export type MapName = 'Dust2' | 'Mirage' | 'Inferno' | 'Nuke' | 'Ancient' | 'Anubis' | 'Cache'

export interface NadeEntry {
  id: string
  mapName: MapName
  type: GrenadeType
  title: string
  description: string | null
  youtubeUrl: string | null
  /** Radar-relative fraction in [0,1], measured from the left edge; null until a pin is placed on the map. */
  landingX: number | null
  /** Radar-relative fraction in [0,1], measured from the top edge; null until a pin is placed on the map. */
  landingY: number | null
  createdByUserId: string
}

export interface AddNadePayload {
  mapName: MapName
  type: GrenadeType
  title: string
  description?: string
  youtubeUrl?: string
}

export interface UpdateNadePositionPayload {
  nadeId: string
  /** Both null clears the pin. */
  x: number | null
  y: number | null
}

export const nadesApi = {
  getNades: (filters: { mapName?: MapName; type?: GrenadeType }) => {
    const params = new URLSearchParams()
    if (filters.mapName) params.set('mapName', filters.mapName)
    if (filters.type) params.set('type', filters.type)
    const query = params.toString()
    return apiClient.get<NadeEntry[]>(`${API_ENDPOINTS.nades}${query ? `?${query}` : ''}`)
  },
  addNade: (payload: AddNadePayload) => apiClient.post<NadeEntry>(API_ENDPOINTS.nades, payload),
  deleteNade: (nadeId: string) => apiClient.delete<void>(API_ENDPOINTS.nadeById(nadeId)),
  updateNadePosition: ({ nadeId, x, y }: UpdateNadePositionPayload) =>
    apiClient.patch<NadeEntry>(API_ENDPOINTS.nadePosition(nadeId), { x, y }),
}
