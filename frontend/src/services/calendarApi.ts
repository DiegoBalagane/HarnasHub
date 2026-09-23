import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export type EventType = 'Training' | 'PickupGame' | 'Match' | 'Tournament' | 'Scrim'
export type AvailabilityStatus = 'Available' | 'Maybe' | 'Unavailable'

export interface CalendarEvent {
  id: string
  title: string
  type: EventType
  startsAtUtc: string
  /** Optional end time, for an event with a real duration (e.g. a training block) rather than a single moment. */
  endsAtUtc: string | null
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
  endsAtUtc?: string | null
  location?: string
  url?: string
  notes?: string
}

export type UpdateEventPayload = CreateEventPayload

export const calendarApi = {
  getUpcomingEvents: (includePast?: boolean) =>
    apiClient.get<CalendarEvent[]>(includePast ? `${API_ENDPOINTS.calendar.events}?includePast=true` : API_ENDPOINTS.calendar.events),
  createEvent: (payload: CreateEventPayload) =>
    apiClient.post<CalendarEvent>(API_ENDPOINTS.calendar.events, payload),
  /** Coach/Manager only — e.g. to fix a wrong date/time after the fact. */
  updateEvent: (eventId: string, payload: UpdateEventPayload) =>
    apiClient.put<CalendarEvent>(API_ENDPOINTS.calendar.event(eventId), payload),
  /** Coach/Manager only; also clears everyone's declared availability for that event. */
  deleteEvent: (eventId: string) => apiClient.delete<void>(API_ENDPOINTS.calendar.event(eventId)),
  getEventAvailability: (eventId: string) =>
    apiClient.get<MemberAvailability[]>(API_ENDPOINTS.calendar.availability(eventId)),
  setAvailability: (eventId: string, status: AvailabilityStatus) =>
    apiClient.post<void>(API_ENDPOINTS.calendar.availability(eventId), { status }),
}
