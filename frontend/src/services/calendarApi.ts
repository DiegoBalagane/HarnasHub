import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export type EventType = 'Training' | 'PickupGame' | 'Match' | 'Tournament' | 'Scrim'
export type AvailabilityStatus = 'Available' | 'Maybe' | 'Unavailable'

export interface CalendarEvent {
  id: string
  title: string
  type: EventType
  startsAtUtc: string
  location: string | null
  /** Optional link to the match/stream/lobby, shown as a clickable link instead of jammed into location. */
  url: string | null
  notes: string | null
}

export interface MemberAvailability {
  userId: string
  displayName: string
  inGameNickname: string | null
  status: AvailabilityStatus | 'NotSet'
}

export interface CreateEventPayload {
  title: string
  type: EventType
  startsAtUtc: string
  location?: string
  url?: string
  notes?: string
}

export const calendarApi = {
  getUpcomingEvents: () => apiClient.get<CalendarEvent[]>(API_ENDPOINTS.calendar.events),
  createEvent: (payload: CreateEventPayload) =>
    apiClient.post<CalendarEvent>(API_ENDPOINTS.calendar.events, payload),
  getEventAvailability: (eventId: string) =>
    apiClient.get<MemberAvailability[]>(API_ENDPOINTS.calendar.availability(eventId)),
  setAvailability: (eventId: string, status: AvailabilityStatus) =>
    apiClient.post<void>(API_ENDPOINTS.calendar.availability(eventId), { status }),
}
