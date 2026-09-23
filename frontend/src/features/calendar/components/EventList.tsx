import { useEffect, useRef, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { useDeleteEvent, useUpcomingEvents, useUpdateEvent } from '../hooks/useCalendar'
import { eventTypeBorderColors, eventTypeColors, eventTypeLabels } from '../labels'
import { AvailabilityPicker } from './AvailabilityPicker'
import { EventForm } from './EventForm'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })
const timeFormatter = new Intl.DateTimeFormat('pl-PL', { timeStyle: 'short' })

/** Lists events (upcoming by default, or every one ever logged); clicking one expands its availability picker.
 * Coach/Manager can also edit or delete an event in place. */
export function EventList() {
  const [includePast, setIncludePast] = useState(false)
  const { data: events, isLoading, isError } = useUpcomingEvents(includePast)
  const [searchParams, setSearchParams] = useSearchParams()
  const linkedEventId = searchParams.get('event')
  const [expandedEventId, setExpandedEventId] = useState<string | null>(linkedEventId)
  const [editingEventId, setEditingEventId] = useState<string | null>(null)
  const [confirmingDeleteId, setConfirmingDeleteId] = useState<string | null>(null)
  const canManage = useIsCoachOrManager()
  const updateEvent = useUpdateEvent()
  const deleteEvent = useDeleteEvent()
  const itemRefs = useRef(new Map<string, HTMLLIElement>())
  const [highlightedEventId, setHighlightedEventId] = useState<string | null>(null)

  // A dashboard/calendar-chip link lands here with ?event=<id> — jump straight to it and open its
  // details instead of leaving the caller to scroll through the whole list to find it.
  useEffect(() => {
    if (!linkedEventId || !events?.some((event) => event.id === linkedEventId)) {
      return
    }

    setExpandedEventId(linkedEventId)
    setHighlightedEventId(linkedEventId)
    itemRefs.current.get(linkedEventId)?.scrollIntoView({ behavior: 'smooth', block: 'center' })

    const nextParams = new URLSearchParams(searchParams)
    nextParams.delete('event')
    setSearchParams(nextParams, { replace: true })

    const clearHighlight = setTimeout(() => setHighlightedEventId(null), 2500)
    return () => clearTimeout(clearHighlight)
    // Runs once the target event is present in the fetched list; the id is consumed immediately after.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [linkedEventId, events])

  return (
    <div className="flex w-full max-w-xl flex-col gap-3">
      <button
        type="button"
        onClick={() => setIncludePast((current) => !current)}
        className="self-start text-xs text-neutral-500 underline-offset-2 hover:text-neutral-300 hover:underline"
      >
        {includePast ? 'Pokaż tylko nadchodzące' : 'Pokaż też przeszłe wydarzenia'}
      </button>

      {isLoading && <p className="text-neutral-400">Ładowanie kalendarza…</p>}
      {isError && <p className="text-red-400">Nie udało się pobrać wydarzeń.</p>}
      {events?.length === 0 && (
        <p className="text-neutral-400">{includePast ? 'Brak wydarzeń.' : 'Brak nadchodzących wydarzeń.'}</p>
      )}

      <ul className="flex flex-col gap-3">
      {events?.map((event) => (
        <li
          key={event.id}
          ref={(node) => {
            if (node) {
              itemRefs.current.set(event.id, node)
            } else {
              itemRefs.current.delete(event.id)
            }
          }}
          className={`rounded-md border border-l-4 p-4 transition ${eventTypeBorderColors[event.type]} ${
            highlightedEventId === event.id ? 'border-red-500 ring-1 ring-red-500/50' : 'border-neutral-800'
          }`}
        >
          {editingEventId === event.id ? (
            <EventForm
              initialValues={{
                title: event.title,
                type: event.type,
                startsAtUtc: event.startsAtUtc,
                endsAtUtc: event.endsAtUtc,
                location: event.location,
                url: event.url,
              }}
              onSubmit={(payload) =>
                updateEvent.mutate(
                  { eventId: event.id, payload },
                  { onSuccess: () => setEditingEventId(null) },
                )
              }
              onCancel={() => setEditingEventId(null)}
              isPending={updateEvent.isPending}
              isError={updateEvent.isError}
              submitLabel="Zapisz zmiany"
            />
          ) : (
            <>
              <button
                className="flex w-full items-center justify-between text-left"
                onClick={() => setExpandedEventId(expandedEventId === event.id ? null : event.id)}
              >
                <div>
                  <p className="font-medium">{event.title}</p>
                  <p className="text-sm text-neutral-400">
                    <span className={eventTypeColors[event.type]}>{eventTypeLabels[event.type]}</span> ·{' '}
                    {dateFormatter.format(new Date(event.startsAtUtc))}
                    {event.endsAtUtc ? ` – ${timeFormatter.format(new Date(event.endsAtUtc))}` : ''}
                    {event.location ? ` · ${event.location}` : ''}
                  </p>
                </div>
                <span className="text-neutral-500">{expandedEventId === event.id ? '▲' : '▼'}</span>
              </button>

              {event.url && (
                <a
                  href={event.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  onClick={(clickEvent) => clickEvent.stopPropagation()}
                  className="mt-1 block truncate text-sm text-blue-400 hover:underline"
                >
                  🔗 {event.url}
                </a>
              )}

              {canManage && (
                <div className="mt-2 flex items-center gap-3 text-xs">
                  <button
                    type="button"
                    onClick={() => setEditingEventId(event.id)}
                    className="text-neutral-400 transition hover:text-neutral-200"
                  >
                    Edytuj
                  </button>
                  {confirmingDeleteId === event.id ? (
                    <span className="flex items-center gap-2 text-neutral-400">
                      Na pewno? Zniknie też dostępność zgłoszona przez wszystkich.
                      <button
                        type="button"
                        disabled={deleteEvent.isPending}
                        onClick={() =>
                          deleteEvent.mutate(event.id, { onSuccess: () => setConfirmingDeleteId(null) })
                        }
                        className="font-medium text-red-400 hover:text-red-300 disabled:opacity-50"
                      >
                        Usuń
                      </button>
                      <button
                        type="button"
                        onClick={() => setConfirmingDeleteId(null)}
                        className="hover:text-neutral-200"
                      >
                        Anuluj
                      </button>
                    </span>
                  ) : (
                    <button
                      type="button"
                      onClick={() => setConfirmingDeleteId(event.id)}
                      className="text-neutral-400 transition hover:text-red-400"
                    >
                      Usuń
                    </button>
                  )}
                </div>
              )}

              {expandedEventId === event.id && (
                <div className="mt-3">
                  <AvailabilityPicker eventId={event.id} />
                </div>
              )}
            </>
          )}
        </li>
      ))}
      </ul>
    </div>
  )
}
