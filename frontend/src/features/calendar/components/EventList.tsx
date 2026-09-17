import { useState } from 'react'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { useDeleteEvent, useUpcomingEvents, useUpdateEvent } from '../hooks/useCalendar'
import { eventTypeLabels } from '../labels'
import { AvailabilityPicker } from './AvailabilityPicker'
import { EventForm } from './EventForm'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })

/** Lists upcoming events; clicking one expands its availability picker. Coach/Manager can also edit or delete an event in place. */
export function EventList() {
  const { data: events, isLoading, isError } = useUpcomingEvents()
  const [expandedEventId, setExpandedEventId] = useState<string | null>(null)
  const [editingEventId, setEditingEventId] = useState<string | null>(null)
  const [confirmingDeleteId, setConfirmingDeleteId] = useState<string | null>(null)
  const canManage = useIsCoachOrManager()
  const updateEvent = useUpdateEvent()
  const deleteEvent = useDeleteEvent()

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
          {editingEventId === event.id ? (
            <EventForm
              initialValues={{
                title: event.title,
                type: event.type,
                startsAtUtc: event.startsAtUtc,
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
                    {eventTypeLabels[event.type]} · {dateFormatter.format(new Date(event.startsAtUtc))}
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
  )
}
