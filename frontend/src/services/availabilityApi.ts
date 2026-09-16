import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

/** Status a player can declare for a single day (the grid also renders the server-only "NotSet" value). */
export type DayAvailabilityStatus = 'Available' | 'PartiallyAvailable' | 'Off'
export type DayStatus = DayAvailabilityStatus | 'NotSet'

/** Minimal effective-status shape the badge renders, shared by the weekly grid and the dashboard. */
export interface DayStatusView {
  status: DayStatus
  /** HH:mm:ss, only set for PartiallyAvailable. */
  from: string | null
  to: string | null
  isVacation: boolean
  note?: string | null
}

export interface DayEntry extends DayStatusView {
  /** Local calendar day in yyyy-MM-dd. */
  date: string
  note: string | null
}

export interface MemberWeek {
  userId: string
  displayName: string
  teamRole: string | null
  /** Exactly 7 entries, ordered from the requested week start. */
  days: DayEntry[]
}

export interface WeekAvailability {
  members: MemberWeek[]
}

export interface Vacation {
  id: string
  startDate: string
  endDate: string
  reason: string | null
}

export interface SetDayAvailabilityPayload {
  date: string
  status: DayAvailabilityStatus
  /** HH:mm, required only for PartiallyAvailable. */
  availableFromLocal: string | null
  availableToLocal: string | null
  note: string | null
}

export interface CreateVacationPayload {
  startDate: string
  endDate: string
  reason: string | null
}

export const availabilityApi = {
  getWeek: (weekStart: string) =>
    apiClient.get<WeekAvailability>(
      `${API_ENDPOINTS.availability.week}?weekStart=${encodeURIComponent(weekStart)}`,
    ),
  setDay: (payload: SetDayAvailabilityPayload) =>
    apiClient.post<void>(API_ENDPOINTS.availability.day, payload),
  getVacations: () => apiClient.get<Vacation[]>(API_ENDPOINTS.availability.vacations),
  createVacation: (payload: CreateVacationPayload) =>
    apiClient.post<Vacation>(API_ENDPOINTS.availability.vacations, payload),
  deleteVacation: (vacationId: string) =>
    apiClient.delete<void>(API_ENDPOINTS.availability.vacationById(vacationId)),
}
