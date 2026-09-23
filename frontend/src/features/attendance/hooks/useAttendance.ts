import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { attendanceApi, type AddIncidentPayload } from '../../../services/attendanceApi'

/** Every roster player's total lateness/absence counts, for the team-wide overview. */
export function useAttendanceSummary() {
  return useQuery({ queryKey: ['attendance', 'summary'], queryFn: attendanceApi.getSummary })
}

/** Logged incidents, newest first, optionally narrowed to one player. */
export function useAttendanceIncidents(userId?: string) {
  return useQuery({
    queryKey: ['attendance', 'incidents', userId ?? 'all'],
    queryFn: () => attendanceApi.getIncidents(userId),
  })
}

/** Logs a lateness/absence entry (Coach/Manager only) and refreshes the summary and incident list. */
export function useAddIncident() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: AddIncidentPayload) => attendanceApi.addIncident(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['attendance'] }),
  })
}

/** Removes a logged incident (Coach/Manager only) and refreshes the summary and incident list. */
export function useDeleteIncident() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (incidentId: string) => attendanceApi.deleteIncident(incidentId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['attendance'] }),
  })
}
