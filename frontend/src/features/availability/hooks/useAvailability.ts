import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  availabilityApi,
  type CreateVacationPayload,
  type SetDayAvailabilityPayload,
} from '../../../services/availabilityApi'

/** Fetches the whole team's effective availability for the week starting at the given yyyy-MM-dd Monday. */
export function useWeekAvailability(weekStartIso: string) {
  return useQuery({
    queryKey: ['availability', 'week', weekStartIso],
    queryFn: () => availabilityApi.getWeek(weekStartIso),
  })
}

/** Declares the current user's availability for one day and refreshes the grid and dashboard. */
export function useSetDayAvailability() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: SetDayAvailabilityPayload) => availabilityApi.setDay(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['availability'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}

/** Fetches the current user's own time-off ranges. */
export function useVacations() {
  return useQuery({
    queryKey: ['availability', 'vacations'],
    queryFn: availabilityApi.getVacations,
  })
}

/** Adds a time-off range for the current user and refreshes availability and dashboard data. */
export function useSetVacation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateVacationPayload) => availabilityApi.createVacation(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['availability'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}

/** Removes one of the current user's time-off ranges and refreshes availability and dashboard data. */
export function useDeleteVacation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (vacationId: string) => availabilityApi.deleteVacation(vacationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['availability'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}
