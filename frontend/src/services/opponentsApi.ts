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
  /** True for opponents the team hid; only returned when the list is requested with hidden ones. */
  isHidden?: boolean
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
  isHidden: boolean
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

/** What deleting an opponent would remove: scouting data always, results and events only on request. */
export interface OpponentDeletePreview {
  notes: number
  demoAnalyses: number
  hasFaceitLink: boolean
  hasReportSnapshot: boolean
  matchResults: number
  events: number
  isHidden: boolean
}

/** Outcome of deleting an opponent; `hidden` when history was kept and the opponent hidden instead. */
export interface DeleteOpponentResult {
  deletedNotes: number
  deletedDemoAnalyses: number
  deletedMatchResults: number
  deletedEvents: number
  hidden: boolean
}

/** Outcome of renaming/merging an opponent; `faceitDataKept` when the target's own FACEIT link won. */
export interface RenameOpponentResult {
  name: string
  updatedNotes: number
  updatedMatchResults: number
  updatedEvents: number
  faceitDataKept: boolean
}

export const opponentManagementApi = {
  getHiddenIncluded: () => apiClient.get<OpponentSummary[]>(API_ENDPOINTS.opponents.listWithHidden),
  getDeletePreview: (name: string) =>
    apiClient.get<OpponentDeletePreview>(API_ENDPOINTS.opponents.deletePreview(name)),
  remove: (name: string, includeHistory: boolean) =>
    apiClient.delete<DeleteOpponentResult>(API_ENDPOINTS.opponents.remove(name, includeHistory)),
  hide: (name: string) => apiClient.post<void>(API_ENDPOINTS.opponents.hide, { name }),
  unhide: (name: string) => apiClient.post<void>(API_ENDPOINTS.opponents.unhide, { name }),
  rename: (from: string, to: string) =>
    apiClient.post<RenameOpponentResult>(API_ENDPOINTS.opponents.rename, { from, to }),
}
