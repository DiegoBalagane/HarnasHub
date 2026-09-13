import { useState } from 'react'
import { useUpcomingEvents } from '../hooks/useCalendar'
import { eventTypeLabels } from '../labels'
import { AvailabilityPicker } from './AvailabilityPicker'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })

/** Lists upcoming events; clicking one expands its availability picker. */
export function EventList() {
  const { data: events, isLoading, isError } = useUpcomingEvents()
  const [expandedEventId, setExpandedEventId] = useState<string | null>(null)

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie kalendarza…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać wydarzeń.</p>
  }

  if (events?.length === 0) {
    return <p className="text-neutral-400">Brak nadchodzących wydarzeń.</p>
  }

  return (
    <ul className="flex w-full max-w-xl flex-col gap-3">
      {events?.map((event) => (
        <li key={event.id} className="rounded-md border border-neutral-800 p-4">
          <button
            className="flex w-full items-center justify-between text-left"
            onClick={() => setExpandedEventId(expandedEventId === event.id ? null : event.id)}
          >
            <div>
              <p className="font-medium">{event.title}</p>
              <p className="text-sm text-neutral-400">
                {eventTypeLabels[event.type]} · {dateFormatter.format(new Date(event.startsAtUtc))}
                {event.location ? ` · ${event.location}` : ''}
              </p>
            </div>
            <span className="text-neutral-500">{expandedEventId === event.id ? '▲' : '▼'}</span>
          </button>

          {expandedEventId === event.id && (
            <div className="mt-3">
              <AvailabilityPicker eventId={event.id} />
            </div>
          )}
        </li>
      ))}
    </ul>
  )
}
