import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export type GrenadeType = 'Smoke' | 'Flash' | 'Molotov' | 'Frag'

export interface NadeEntry {
  id: string
  mapName: string
  type: GrenadeType
  title: string
  description: string | null
  youtubeUrl: string | null
  createdByUserId: string
}

export interface AddNadePayload {
  mapName: string
  type: GrenadeType
  title: string
  description?: string
  youtubeUrl?: string
}

export const nadesApi = {
  getNades: (filters: { mapName?: string; type?: GrenadeType }) => {
    const params = new URLSearchParams()
    if (filters.mapName) params.set('mapName', filters.mapName)
    if (filters.type) params.set('type', filters.type)
    const query = params.toString()
    return apiClient.get<NadeEntry[]>(`${API_ENDPOINTS.nades}${query ? `?${query}` : ''}`)
  },
  addNade: (payload: AddNadePayload) => apiClient.post<NadeEntry>(API_ENDPOINTS.nades, payload),
  deleteNade: (nadeId: string) => apiClient.delete<void>(API_ENDPOINTS.nadeById(nadeId)),
}
