import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

/** Per-channel Discord webhook flags (true when the channel has its own webhook or the Discord__WebhookUrl fallback). */
export interface DiscordChannelsStatus {
  announcements: boolean
  matchSchedule: boolean
  demoReview: boolean
  opponentScouting: boolean
}

/** Which optional integrations are configured on this deployment — flags only, never the secret values. */
export interface AdminStatus {
  faceitApiKeyConfigured: boolean
  faceitDownloadsTokenConfigured: boolean
  s3Configured: boolean
  discordWebhookConfigured: boolean
  frontendBaseUrlConfigured: boolean
  discordChannels: DiscordChannelsStatus
}

export const adminApi = {
  /** Manager only. */
  getStatus: () => apiClient.get<AdminStatus>(API_ENDPOINTS.admin.status),
}
