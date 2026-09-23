import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  calendarApi,
  type AvailabilityStatus,
  type CreateEventPayload,
  type UpdateEventPayload,
} from '../../../services/calendarApi'

/** Fetches every upcoming event, soonest first — or, with includePast, every event ever logged, most recent first. */
export function useUpcomingEvents(includePast = false) {
  return useQuery({
    queryKey: ['calendar', 'events', includePast],
    queryFn: () => calendarApi.getUpcomingEvents(includePast),
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

/** Edits an existing event (e.g. to fix a wrong time) and refreshes the events list and dashboard. */
export function useUpdateEvent() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ eventId, payload }: { eventId: string; payload: UpdateEventPayload }) =>
      calendarApi.updateEvent(eventId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['calendar', 'events'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}

/** Deletes an event and refreshes the events list and dashboard. */
export function useDeleteEvent() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (eventId: string) => calendarApi.deleteEvent(eventId),
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
