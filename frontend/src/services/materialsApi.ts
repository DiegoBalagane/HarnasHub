import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export interface TrainingMaterial {
  id: string
  title: string
  url: string
  category: string | null
  description: string | null
}

export interface AddMaterialPayload {
  title: string
  url: string
  category?: string
  description?: string
}

export const materialsApi = {
  getMaterials: () => apiClient.get<TrainingMaterial[]>(API_ENDPOINTS.trainingMaterials),
  addMaterial: (payload: AddMaterialPayload) =>
    apiClient.post<TrainingMaterial>(API_ENDPOINTS.trainingMaterials, payload),
}
