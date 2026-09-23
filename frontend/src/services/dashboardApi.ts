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
  /** 'Main' | 'Bench' | null (unassigned) — grouped the same way as the availability calendar. */
  rosterSlot: string | null
  /** Always sorted into its own group at the bottom, regardless of roster slot. */
  isCoach: boolean
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

/** The current user's own average rating over their most recent stat lines — null when they have none yet. */
export interface MyRecentPerformance {
  avgRating: number
  matchesCounted: number
}

/** The team's most recently logged result, for a quick "how did we do last time" glance. */
export interface LastMatchResult {
  matchResultId: string
  opponent: string
  ourScore: number
  opponentScore: number
  won: boolean
  playedAtUtc: string
  mapName: string | null
}

/** Team-wide lateness/absence totals over the trailing 30 days, for the dashboard tile. */
export interface TeamAttendanceSummary {
  lateCount: number
  absentCount: number
}

export interface DashboardSummary {
  nextEvent: CalendarEvent | null
  openTaskCount: number
  today: DailyTeamStatus
  tomorrow: DailyTeamStatus
  myRecentPerformance: MyRecentPerformance | null
  lastMatch: LastMatchResult | null
  attendance: TeamAttendanceSummary
}

export const dashboardApi = {
  getSummary: () => apiClient.get<DashboardSummary>(API_ENDPOINTS.dashboard),
}
