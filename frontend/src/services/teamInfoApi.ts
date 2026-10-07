import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

/** One entry of the team Info page (Discord invite, server address, config snippet…). */
export interface TeamInfoEntry {
  id: string
  category: string
  title: string
  value: string
  /** Masked in the UI until a member reveals it (e.g. server passwords). */
  isSecret: boolean
  sortOrder: number
  updatedAtUtc: string
}

export interface TeamInfoPayload {
  category: string
  title: string
  value: string
  isSecret: boolean
}

export const teamInfoApi = {
  list: () => apiClient.get<TeamInfoEntry[]>(API_ENDPOINTS.teamInfo.list),
  create: (payload: TeamInfoPayload) => apiClient.post<TeamInfoEntry>(API_ENDPOINTS.teamInfo.list, payload),
  update: (id: string, payload: TeamInfoPayload) =>
    apiClient.put<TeamInfoEntry>(API_ENDPOINTS.teamInfo.byId(id), payload),
  remove: (id: string) => apiClient.delete<void>(API_ENDPOINTS.teamInfo.byId(id)),
  /** Sets the order of one category's entries to the given id sequence. */
  reorder: (orderedIds: string[]) => apiClient.put<void>(API_ENDPOINTS.teamInfo.order, { orderedIds }),
}
