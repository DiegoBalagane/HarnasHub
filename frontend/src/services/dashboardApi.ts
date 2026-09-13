import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { CalendarEvent } from './calendarApi'

export interface DashboardSummary {
  nextEvent: CalendarEvent | null
  openTaskCount: number
}

export const dashboardApi = {
  getSummary: () => apiClient.get<DashboardSummary>(API_ENDPOINTS.dashboard),
}
