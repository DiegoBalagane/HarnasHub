import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export interface OpponentNote {
  id: string
  opponentName: string
  content: string
  materialUrl: string | null
  createdAtUtc: string
}

export interface AddOpponentNotePayload {
  opponentName: string
  content: string
  materialUrl?: string
}

export const opponentNotesApi = {
  getNotes: (opponentName?: string) => {
    const query = opponentName ? `?opponentName=${encodeURIComponent(opponentName)}` : ''
    return apiClient.get<OpponentNote[]>(`${API_ENDPOINTS.opponents}${query}`)
  },
  addNote: (payload: AddOpponentNotePayload) =>
    apiClient.post<OpponentNote>(API_ENDPOINTS.opponents, payload),
}
