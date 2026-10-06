import { useState } from 'react'
import { DateTimePicker } from '../../../components/ui/DateTimePicker'
import { useModalGuard } from '../../../components/ModalGuardContext'
import { isoUtcToLocalValue, localValueToIsoUtc } from '../../../components/ui/dateTime'
import type { CreateEventPayload, EventType } from '../../../services/calendarApi'
import { OpponentNameInput } from '../../opponents/components/OpponentNameInput'
import { eventTypeLabels } from '../labels'

const eventTypes: EventType[] = ['Training', 'PickupGame', 'Match', 'Tournament', 'Scrim']
/** Event types played against a specific opposing team — only these get an opponent field. */
const opponentEventTypes: EventType[] = ['Match', 'Tournament', 'Scrim']
const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

export interface EventFormInitialValues {
  title: string
  type: EventType
  startsAtUtc: string
  endsAtUtc: string | null
  location: string | null
  url: string | null
  opponent: string | null
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
    isoUtcToLocalValue(initialValues?.startsAtUtc),
  )
  const [endsAt, setEndsAt] = useState(
    isoUtcToLocalValue(initialValues?.endsAtUtc),
  )
  const [location, setLocation] = useState(initialValues?.location ?? '')
  const [url, setUrl] = useState(initialValues?.url ?? '')
  const [opponent, setOpponent] = useState(initialValues?.opponent ?? '')
  const hasOpponent = opponentEventTypes.includes(type)

  const fieldsSnapshot = JSON.stringify([title, type, startsAt, endsAt, location, url, opponent])
  const [initialSnapshot] = useState(fieldsSnapshot)
  useModalGuard({ isBusy: isPending, isDirty: fieldsSnapshot !== initialSnapshot })

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    onSubmit({
      title,
      type,
      startsAtUtc: localValueToIsoUtc(startsAt) ?? new Date().toISOString(),
      endsAtUtc: localValueToIsoUtc(endsAt),
      location: location || undefined,
      url: url || undefined,
      opponent: hasOpponent && opponent.trim() ? opponent : null,
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
          <DateTimePicker label="Początek" placeholder="Wybierz termin" value={startsAt} onChange={setStartsAt} className={inputClass} />
        </label>
        <label className="flex flex-1 flex-col gap-1 text-xs text-neutral-500">
          Koniec (opcjonalnie)
          <DateTimePicker label="Koniec" placeholder="Brak końca" clearable value={endsAt} onChange={setEndsAt} className={inputClass} />
        </label>
      </div>

      {hasOpponent && (
        <OpponentNameInput
          value={opponent}
          onChange={setOpponent}
          placeholder="Przeciwnik (opcjonalnie) — podepnie profil przeciwnika"
          className={inputClass}
        />
      )}

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

      {isError && <p className="text-sm text-danger-400">Nie udało się zapisać wydarzenia.</p>}

      <div className="flex gap-2">
        <button
          type="submit"
          disabled={isPending || startsAt === ''}
          className="self-start rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
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
