import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export interface RefreshSessionResponse {
  accessToken: string
  userId: string
  displayName: string
  role: string
  avatarUrl: string | null
}

export const authApi = {
  /** Re-issues the caller's JWT from their current DB role — lets a promoted Guest pick up team access without a fresh Discord login. */
  refresh: () => apiClient.post<RefreshSessionResponse>(API_ENDPOINTS.auth.refresh, {}),
}
