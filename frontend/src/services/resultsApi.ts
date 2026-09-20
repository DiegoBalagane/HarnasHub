import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { LeagueType } from './leaguesApi'

export type MatchCategory = 'Scrimmage' | 'League' | 'Tournament'

export interface MatchResult {
  id: string
  opponent: string
  ourScore: number
  opponentScore: number
  mapName: string | null
  demoUrl: string | null
  notes: string | null
  playedAtUtc: string
  category: MatchCategory
  tournamentId: string | null
  tournamentName: string | null
  leagueId: string | null
  leagueName: string | null
  leagueSeason: string | null
  leagueType: LeagueType | null
}

export interface AddResultPayload {
  opponent: string
  /** Optional: computed from the attached demo, and only needed manually when there's no demo (or none of its rounds match our roster). */
  ourScore?: number
  opponentScore?: number
  /** Optional: overwritten by the demo's own map when one is attached. */
  mapName?: string
  /** An external link to the demo for download — unrelated to demoFile, which is parsed server-side and never stored. */
  demoUrl?: string
  notes?: string
  playedAtUtc: string
  category: MatchCategory
  tournamentId?: string | null
  leagueId?: string | null
  /** Parsed in memory on the server to derive the score and map; the file itself is never persisted. */
  demoFile?: File | null
}

/** Serialises the payload as multipart/form-data so an optional .dem can ride along with the manual fields. */
function toFormData(payload: AddResultPayload): FormData {
  const formData = new FormData()
  formData.append('opponent', payload.opponent)
  formData.append('playedAtUtc', payload.playedAtUtc)
  formData.append('category', payload.category)

  if (payload.ourScore !== undefined) formData.append('ourScore', String(payload.ourScore))
  if (payload.opponentScore !== undefined) formData.append('opponentScore', String(payload.opponentScore))
  if (payload.mapName) formData.append('mapName', payload.mapName)
  if (payload.demoUrl) formData.append('demoUrl', payload.demoUrl)
  if (payload.notes) formData.append('notes', payload.notes)
  if (payload.tournamentId) formData.append('tournamentId', payload.tournamentId)
  if (payload.leagueId) formData.append('leagueId', payload.leagueId)
  if (payload.demoFile) formData.append('demo', payload.demoFile)

  return formData
}

export const resultsApi = {
  getResults: () => apiClient.get<MatchResult[]>(API_ENDPOINTS.results),
  addResult: (payload: AddResultPayload) =>
    apiClient.postForm<MatchResult>(API_ENDPOINTS.results, toFormData(payload)),
}
