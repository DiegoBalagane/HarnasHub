import { useState } from 'react'
import type { EventType } from '../../../services/calendarApi'
import { useCreateEvent } from '../hooks/useCalendar'
import { eventTypeLabels } from '../labels'

const eventTypes: EventType[] = ['Training', 'PickupGame', 'Match', 'Tournament', 'Scrim']

/** Coach/Manager-only form for scheduling a new calendar event. */
export function CreateEventForm() {
  const [title, setTitle] = useState('')
  const [type, setType] = useState<EventType>('Training')
  const [startsAt, setStartsAt] = useState('')
  const [location, setLocation] = useState('')
  const [url, setUrl] = useState('')
  const createEvent = useCreateEvent()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    createEvent.mutate(
      {
        title,
        type,
        startsAtUtc: new Date(startsAt).toISOString(),
        location: location || undefined,
        url: url || undefined,
      },
      {
        onSuccess: () => {
          setTitle('')
          setLocation('')
          setUrl('')
          setStartsAt('')
        },
      },
    )
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex w-full max-w-xl flex-col gap-3 rounded-md border border-neutral-800 p-4"
    >
      <h2 className="font-medium">Dodaj wydarzenie</h2>

      <div className="flex gap-3">
        <input
          required
          placeholder="Tytuł"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <select
          value={type}
          onChange={(event) => setType(event.target.value as EventType)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          {eventTypes.map((eventType) => (
            <option key={eventType} value={eventType}>
              {eventTypeLabels[eventType]}
            </option>
          ))}
        </select>
      </div>

      <div className="flex gap-3">
        <input
          required
          type="datetime-local"
          value={startsAt}
          onChange={(event) => setStartsAt(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <input
          placeholder="Lokalizacja (opcjonalnie)"
          value={location}
          onChange={(event) => setLocation(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
      </div>

      <input
        type="url"
        placeholder="Link do meczu/streamu (opcjonalnie)"
        value={url}
        onChange={(event) => setUrl(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {createEvent.isError && <p className="text-sm text-red-400">Nie udało się dodać wydarzenia.</p>}

      <button
        type="submit"
        disabled={createEvent.isPending}
        className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {createEvent.isPending ? 'Dodawanie…' : 'Dodaj wydarzenie'}
      </button>
    </form>
  )
}
