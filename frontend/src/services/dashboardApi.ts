import { API_ENDPOINTS } from '../constants'
import type { DayStatus } from './availabilityApi'
import { apiClient } from './apiClient'
import type { CalendarEvent } from './calendarApi'

/** One member's effective availability for a single day, as shown on the dashboard. */
export interface MemberDayStatus {
  userId: string
  displayName: string
  inGameNickname: string | null
  teamRole: string | null
  status: DayStatus
  /** HH:mm:ss, only set for PartiallyAvailable. */
  from: string | null
  to: string | null
  isVacation: boolean
}

/** Who is available on one day, plus that day's earliest event if there is one. */
export interface DailyTeamStatus {
  /** Calendar day in yyyy-MM-dd. */
  date: string
  members: MemberDayStatus[]
  event: CalendarEvent | null
}

export interface DashboardSummary {
  nextEvent: CalendarEvent | null
  openTaskCount: number
  today: DailyTeamStatus
  tomorrow: DailyTeamStatus
}

export const dashboardApi = {
  getSummary: () => apiClient.get<DashboardSummary>(API_ENDPOINTS.dashboard),
}
