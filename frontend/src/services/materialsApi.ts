import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export type MaterialCategory = 'Grenades' | 'Tactics' | 'Aim' | 'Positioning' | 'VodReview' | 'Communication' | 'Other'

export interface TrainingMaterial {
  id: string
  title: string
  url: string
  category: MaterialCategory | null
  description: string | null
}

export interface AddMaterialPayload {
  title: string
  url: string
  category?: MaterialCategory
  description?: string
}

export const materialsApi = {
  getMaterials: () => apiClient.get<TrainingMaterial[]>(API_ENDPOINTS.trainingMaterials),
  addMaterial: (payload: AddMaterialPayload) =>
    apiClient.post<TrainingMaterial>(API_ENDPOINTS.trainingMaterials, payload),
}
