import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '../../../../components/ui/Button'
import type { CalendarEvent } from '../../../../services/calendarApi'
import { useIsCoachOrManager } from '../../../auth/hooks/useIsCoachOrManager'
import { opponentProfilePath } from '../../../opponents/paths'
import { useDeleteEvent, useUpdateEvent } from '../../hooks/useCalendar'
import { eventTypeColors, eventTypeLabels } from '../../labels'
import { EventDetails } from '../EventDetails'
import { EventForm } from '../EventForm'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'full', timeStyle: 'short' })
const timeFormatter = new Intl.DateTimeFormat('pl-PL', { timeStyle: 'short' })

interface EventDetailsPanelProps {
  event: CalendarEvent
  onClose: () => void
}

/** Side drawer (bottom-full on phones) with an event's details, availability, game plan and veto; Coach/Manager can edit or delete it. */
export function EventDetailsPanel({ event, onClose }: EventDetailsPanelProps) {
  const canManage = useIsCoachOrManager()
  const updateEvent = useUpdateEvent()
  const deleteEvent = useDeleteEvent()
  const [isEditing, setIsEditing] = useState(false)
  const [isConfirmingDelete, setIsConfirmingDelete] = useState(false)

  useEffect(() => {
    function handleKeyDown(keyEvent: KeyboardEvent) {
      if (keyEvent.key === 'Escape') onClose()
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [onClose])

  return (
    <div role="presentation" className="fixed inset-0 z-50 flex justify-end bg-black/60" onClick={onClose}>
      <aside
        role="dialog"
        aria-modal="true"
        aria-label={`Szczegóły: ${event.title}`}
        onClick={(clickEvent) => clickEvent.stopPropagation()}
        className="flex h-full w-full flex-col gap-3 overflow-y-auto border-l border-surface-border bg-neutral-950 p-5 shadow-xl sm:max-w-md"
      >
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <p className={`text-xs font-medium ${eventTypeColors[event.type]}`}>{eventTypeLabels[event.type]}</p>
            <h2 className="text-lg font-semibold text-white">{event.title}</h2>
          </div>
          <button type="button" onClick={onClose} title="Zamknij" className="text-neutral-500 hover:text-neutral-200">
            ✕
          </button>
        </div>

        {isEditing ? (
          <EventForm
            initialValues={{
              title: event.title,
              type: event.type,
              startsAtUtc: event.startsAtUtc,
              endsAtUtc: event.endsAtUtc,
              location: event.location,
              url: event.url,
              opponent: event.opponent,
            }}
            onSubmit={(payload) =>
              updateEvent.mutate({ eventId: event.id, payload }, { onSuccess: () => setIsEditing(false) })
            }
            onCancel={() => setIsEditing(false)}
            isPending={updateEvent.isPending}
            isError={updateEvent.isError}
            submitLabel="Zapisz zmiany"
          />
        ) : (
          <>
            <p className="text-sm text-neutral-300">
              {dateFormatter.format(new Date(event.startsAtUtc))}
              {event.endsAtUtc ? ` – ${timeFormatter.format(new Date(event.endsAtUtc))}` : ''}
              {event.location ? ` · ${event.location}` : ''}
            </p>
            {event.url && (
              <a
                href={event.url}
                target="_blank"
                rel="noopener noreferrer"
                className="truncate text-sm text-info-400 hover:underline"
              >
                🔗 {event.url}
              </a>
            )}
            {event.opponent && (
              <Link to={opponentProfilePath(event.opponent)} className="text-sm text-primary-400 hover:underline">
                🎯 vs {event.opponent} — profil przeciwnika
              </Link>
            )}

            {canManage && (
              <div className="flex flex-wrap items-center gap-2">
                <Button variant="secondary" size="sm" onClick={() => setIsEditing(true)}>
                  Edytuj
                </Button>
                {isConfirmingDelete ? (
                  <span className="flex items-center gap-2 text-xs text-neutral-400">
                    Na pewno? Zniknie też dostępność zgłoszona przez wszystkich.
                    <Button
                      variant="danger"
                      size="sm"
                      disabled={deleteEvent.isPending}
                      onClick={() => deleteEvent.mutate(event.id, { onSuccess: onClose })}
                    >
                      Usuń
                    </Button>
                    <Button variant="ghost" size="sm" onClick={() => setIsConfirmingDelete(false)}>
                      Anuluj
                    </Button>
                  </span>
                ) : (
                  <Button variant="ghost" size="sm" onClick={() => setIsConfirmingDelete(true)}>
                    Usuń
                  </Button>
                )}
              </div>
            )}

            <EventDetails event={event} />
          </>
        )}
      </aside>
    </div>
  )
}
