import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { calendarApi, type AvailabilityStatus, type CreateEventPayload } from '../../../services/calendarApi'

/** Fetches every upcoming event, soonest first. */
export function useUpcomingEvents() {
  return useQuery({
    queryKey: ['calendar', 'events'],
    queryFn: calendarApi.getUpcomingEvents,
  })
}

/** Creates a new event and refreshes the events list and dashboard. */
export function useCreateEvent() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateEventPayload) => calendarApi.createEvent(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['calendar', 'events'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}

/** Fetches every team member's availability for one event. */
export function useEventAvailability(eventId: string, enabled: boolean) {
  return useQuery({
    queryKey: ['calendar', 'events', eventId, 'availability'],
    queryFn: () => calendarApi.getEventAvailability(eventId),
    enabled,
  })
}

/** Declares the current user's availability for an event and refreshes that event's availability list. */
export function useSetAvailability(eventId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (status: AvailabilityStatus) => calendarApi.setAvailability(eventId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['calendar', 'events', eventId, 'availability'] })
    },
  })
}
