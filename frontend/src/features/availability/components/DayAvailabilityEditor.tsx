import { useState } from 'react'
import type { DayAvailabilityStatus, DayEntry } from '../../../services/availabilityApi'
import { useSetDayAvailability } from '../hooks/useAvailability'
import { dayStatusLabels } from '../labels'
import { parseIsoDate, toShortTime } from '../weekDates'

const editableStatuses: DayAvailabilityStatus[] = ['Available', 'PartiallyAvailable', 'Off']
const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'full' })
const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

interface DayAvailabilityEditorProps {
  /** Day being edited, as yyyy-MM-dd. */
  date: string
  /** Current entry for that day, used to prefill the form. */
  entry: DayEntry | undefined
  onClose: () => void
}

/** Inline form for declaring the current user's availability on a single day: every status/time change saves immediately, only the note keeps an explicit Save/Delete. */
export function DayAvailabilityEditor({ date, entry, onClose }: DayAvailabilityEditorProps) {
  // No status is pre-selected for a day that has nothing declared yet — a highlighted button there
  // would look like something was already saved when nothing was.
  const [status, setStatus] = useState<DayAvailabilityStatus | null>(
    entry && entry.status !== 'NotSet' ? entry.status : null,
  )
  const [from, setFrom] = useState(toShortTime(entry?.from ?? null) ?? '18:00')
  const [to, setTo] = useState(toShortTime(entry?.to ?? null) ?? '22:00')
  const [note, setNote] = useState(entry?.note ?? '')
  const [validationError, setValidationError] = useState<string | null>(null)
  const setDayAvailability = useSetDayAvailability()

  const isPartial = status === 'PartiallyAvailable'

  function saveStatus(newStatus: DayAvailabilityStatus, times?: { from: string; to: string }) {
    const effectiveFrom = newStatus === 'PartiallyAvailable' ? (times?.from ?? from) : null
    const effectiveTo = newStatus === 'PartiallyAvailable' ? (times?.to ?? to) : null

    if (effectiveFrom !== null && effectiveTo !== null && effectiveFrom >= effectiveTo) {
      setValidationError('Godzina od musi być wcześniejsza niż godzina do.')
      return
    }

    setValidationError(null)
    setStatus(newStatus)
    setDayAvailability.mutate({
      date,
      status: newStatus,
      availableFromLocal: effectiveFrom,
      availableToLocal: effectiveTo,
      note: note.trim() === '' ? null : note.trim(),
    })
  }

  function handleTimeBlur() {
    if (status === 'PartiallyAvailable') {
      saveStatus('PartiallyAvailable', { from, to })
    }
  }

  function saveNote(nextNote: string | null) {
    if (!status) return

    setDayAvailability.mutate({
      date,
      status,
      availableFromLocal: isPartial ? from : null,
      availableToLocal: isPartial ? to : null,
      note: nextNote,
    })
  }

  return (
    <div className="flex w-full max-w-md flex-col gap-3 rounded-md border border-neutral-700 bg-neutral-950 p-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium">{dateFormatter.format(parseIsoDate(date))}</h3>
        <button type="button" onClick={onClose} className="text-xs text-neutral-500 hover:text-neutral-300">
          Zamknij
        </button>
      </div>

      <div className="flex flex-wrap gap-2">
        {editableStatuses.map((option) => (
          <button
            key={option}
            type="button"
            onClick={() => saveStatus(option)}
            className={`rounded-md border px-3 py-1 text-xs transition ${
              status === option
                ? 'border-neutral-400 bg-neutral-800 text-neutral-100'
                : 'border-neutral-700 text-neutral-400 hover:border-neutral-500'
            }`}
          >
            {dayStatusLabels[option]}
          </button>
        ))}
      </div>

      {isPartial && (
        <div className="flex items-center gap-2">
          <input
            required
            type="time"
            value={from}
            onChange={(event) => setFrom(event.target.value)}
            onBlur={handleTimeBlur}
            className={`flex-1 ${inputClass}`}
          />
          <span className="text-xs text-neutral-500">–</span>
          <input
            required
            type="time"
            value={to}
            onChange={(event) => setTo(event.target.value)}
            onBlur={handleTimeBlur}
            className={`flex-1 ${inputClass}`}
          />
        </div>
      )}

      <div className="flex flex-col gap-2">
        <input
          maxLength={300}
          placeholder={status ? 'Notatka (opcjonalnie)' : 'Wybierz najpierw status'}
          value={note}
          disabled={!status}
          onChange={(event) => setNote(event.target.value)}
          className={`${inputClass} disabled:opacity-50`}
        />

        <div className="flex gap-2">
          <button
            type="button"
            disabled={!status || setDayAvailability.isPending}
            onClick={() => saveNote(note.trim() === '' ? null : note.trim())}
            className="rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
          >
            Zapisz notatkę
          </button>
          {entry?.note && (
            <button
              type="button"
              disabled={setDayAvailability.isPending}
              onClick={() => {
                setNote('')
                saveNote(null)
              }}
              className="rounded-md border border-neutral-700 px-3 py-1.5 text-xs text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
            >
              Usuń notatkę
            </button>
          )}
        </div>
      </div>

      {validationError !== null && <p className="text-sm text-red-400">{validationError}</p>}
      {setDayAvailability.isError && (
        <p className="text-sm text-red-400">Nie udało się zapisać dostępności.</p>
      )}
    </div>
  )
}
