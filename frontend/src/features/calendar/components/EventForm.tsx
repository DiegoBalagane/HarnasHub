import { useState } from 'react'
import type { CreateEventPayload, EventType } from '../../../services/calendarApi'
import { eventTypeLabels } from '../labels'

const eventTypes: EventType[] = ['Training', 'PickupGame', 'Match', 'Tournament', 'Scrim']
const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

/** Converts a UTC ISO string to the local yyyy-MM-ddTHH:mm shape a datetime-local input expects. */
function toLocalInputValue(isoUtc: string): string {
  const date = new Date(isoUtc)
  const offsetMs = date.getTimezoneOffset() * 60_000
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16)
}

export interface EventFormInitialValues {
  title: string
  type: EventType
  startsAtUtc: string
  endsAtUtc: string | null
  location: string | null
  url: string | null
}

interface EventFormProps {
  initialValues?: EventFormInitialValues
  onSubmit: (payload: CreateEventPayload) => void
  onCancel?: () => void
  isPending: boolean
  isError: boolean
  submitLabel: string
}

/** Shared title/type/time/location/link fields for both creating and editing a calendar event. */
export function EventForm({ initialValues, onSubmit, onCancel, isPending, isError, submitLabel }: EventFormProps) {
  const [title, setTitle] = useState(initialValues?.title ?? '')
  const [type, setType] = useState<EventType>(initialValues?.type ?? 'Training')
  const [startsAt, setStartsAt] = useState(
    initialValues ? toLocalInputValue(initialValues.startsAtUtc) : '',
  )
  const [endsAt, setEndsAt] = useState(
    initialValues?.endsAtUtc ? toLocalInputValue(initialValues.endsAtUtc) : '',
  )
  const [location, setLocation] = useState(initialValues?.location ?? '')
  const [url, setUrl] = useState(initialValues?.url ?? '')

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    onSubmit({
      title,
      type,
      startsAtUtc: new Date(startsAt).toISOString(),
      endsAtUtc: endsAt ? new Date(endsAt).toISOString() : null,
      location: location || undefined,
      url: url || undefined,
    })
  }

  return (
    <form onSubmit={handleSubmit} className="flex w-full max-w-xl flex-col gap-3">
      <div className="flex gap-3">
        <input
          required
          placeholder="Tytuł"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          className={`flex-1 ${inputClass}`}
        />
        <select value={type} onChange={(event) => setType(event.target.value as EventType)} className={inputClass}>
          {eventTypes.map((eventType) => (
            <option key={eventType} value={eventType}>
              {eventTypeLabels[eventType]}
            </option>
          ))}
        </select>
      </div>

      <div className="flex flex-wrap gap-3">
        <label className="flex flex-1 flex-col gap-1 text-xs text-neutral-500">
          Początek
          <input
            required
            type="datetime-local"
            value={startsAt}
            onChange={(event) => setStartsAt(event.target.value)}
            className={inputClass}
          />
        </label>
        <label className="flex flex-1 flex-col gap-1 text-xs text-neutral-500">
          Koniec (opcjonalnie)
          <input
            type="datetime-local"
            value={endsAt}
            onChange={(event) => setEndsAt(event.target.value)}
            className={inputClass}
          />
        </label>
      </div>

      <input
        placeholder="Lokalizacja (opcjonalnie)"
        value={location}
        onChange={(event) => setLocation(event.target.value)}
        className={inputClass}
      />

      <input
        type="url"
        placeholder="Link do meczu/streamu (opcjonalnie)"
        value={url}
        onChange={(event) => setUrl(event.target.value)}
        className={inputClass}
      />

      {isError && <p className="text-sm text-red-400">Nie udało się zapisać wydarzenia.</p>}

      <div className="flex gap-2">
        <button
          type="submit"
          disabled={isPending}
          className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
        >
          {isPending ? 'Zapisywanie…' : submitLabel}
        </button>
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            className="self-start rounded-md border border-neutral-700 px-4 py-2 text-sm text-neutral-300 transition hover:border-neutral-500"
          >
            Anuluj
          </button>
        )}
      </div>
    </form>
  )
}
