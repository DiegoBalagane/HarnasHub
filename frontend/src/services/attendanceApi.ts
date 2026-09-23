import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export type AttendanceIncidentType = 'Late' | 'Absent'

/** One logged lateness/absence entry. */
export interface AttendanceIncident {
  id: string
  userId: string
  playerName: string
  type: AttendanceIncidentType
  occurredOn: string
  note: string | null
  createdAtUtc: string
}

/** A roster player's total lateness/absence counts, for the team-wide overview every member can see. */
export interface AttendanceSummaryEntry {
  userId: string
  playerName: string
  lateCount: number
  absentCount: number
}

export interface AddIncidentPayload {
  userId: string
  type: AttendanceIncidentType
  occurredOn: string
  note?: string | null
}

export const attendanceApi = {
  getSummary: () => apiClient.get<AttendanceSummaryEntry[]>(API_ENDPOINTS.attendance.summary),
  getIncidents: (userId?: string) => apiClient.get<AttendanceIncident[]>(API_ENDPOINTS.attendance.incidents(userId)),
  addIncident: (payload: AddIncidentPayload) =>
    apiClient.post<AttendanceIncident>(API_ENDPOINTS.attendance.incidents(), payload),
  deleteIncident: (incidentId: string) => apiClient.delete<void>(API_ENDPOINTS.attendance.incidentById(incidentId)),
}
