import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

/** Which optional integrations are configured on this deployment — flags only, never the secret values. */
export interface AdminStatus {
  faceitApiKeyConfigured: boolean
  faceitDownloadsTokenConfigured: boolean
  s3Configured: boolean
  discordWebhookConfigured: boolean
  frontendBaseUrlConfigured: boolean
}

export const adminApi = {
  /** Manager only. */
  getStatus: () => apiClient.get<AdminStatus>(API_ENDPOINTS.admin.status),
}
