import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { CalendarEvent } from './calendarApi'
import type { MatchCategory } from './resultsApi'

export interface OpponentNote {
  id: string
  opponentName: string
  content: string
  materialUrl: string | null
  createdAtUtc: string
}

export interface OpponentNotePayload {
  opponentName: string
  content: string
  materialUrl?: string
}

/** One opponent on the list — any team name seen in notes, results or events, with the head-to-head record. */
export interface OpponentSummary {
  name: string
  noteCount: number
  wins: number
  losses: number
  draws: number
  lastPlayedAtUtc: string | null
  /** Next scheduled game against them, null when none is planned. */
  nextEventAtUtc: string | null
}

export interface OpponentMapRecord {
  mapName: string
  wins: number
  losses: number
  draws: number
}

export interface OpponentMatch {
  matchResultId: string
  playedAtUtc: string
  mapName: string | null
  ourScore: number
  opponentScore: number
  category: MatchCategory
  demoUrl: string | null
  notes: string | null
}

/** Everything the team knows about one opponent. */
export interface OpponentProfile {
  name: string
  wins: number
  losses: number
  draws: number
  maps: OpponentMapRecord[]
  matches: OpponentMatch[]
  notes: OpponentNote[]
  upcomingEvents: CalendarEvent[]
}

export const opponentsApi = {
  getOpponents: () => apiClient.get<OpponentSummary[]>(API_ENDPOINTS.opponents.list),
  getProfile: (name: string) => apiClient.get<OpponentProfile>(API_ENDPOINTS.opponents.profile(name)),
  addNote: (payload: OpponentNotePayload) =>
    apiClient.post<OpponentNote>(API_ENDPOINTS.opponents.notes, payload),
  updateNote: (noteId: string, payload: OpponentNotePayload) =>
    apiClient.put<OpponentNote>(API_ENDPOINTS.opponents.noteById(noteId), payload),
  deleteNote: (noteId: string) => apiClient.delete<void>(API_ENDPOINTS.opponents.noteById(noteId)),
}
