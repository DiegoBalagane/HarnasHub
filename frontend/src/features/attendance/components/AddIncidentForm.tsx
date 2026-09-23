import { useState } from 'react'
import { useRoster } from '../../roster/hooks/useRoster'
import type { AttendanceIncidentType } from '../../../services/attendanceApi'
import { useAddIncident } from '../hooks/useAttendance'

const todayIso = () => new Date().toISOString().slice(0, 10)

/** Coach/Manager-only form to log a lateness or absence for a roster player on a training day. */
export function AddIncidentForm() {
  const { data: roster } = useRoster()
  const addIncident = useAddIncident()

  const [userId, setUserId] = useState('')
  const [type, setType] = useState<AttendanceIncidentType>('Late')
  const [occurredOn, setOccurredOn] = useState(todayIso())
  const [note, setNote] = useState('')

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    addIncident.mutate(
      { userId, type, occurredOn, note: note.trim() === '' ? null : note.trim() },
      {
        onSuccess: () => {
          setUserId('')
          setNote('')
        },
      },
    )
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-wrap items-end gap-2 rounded-md border border-neutral-800 p-3">
      <label className="flex flex-col gap-1 text-xs text-neutral-400">
        Zawodnik
        <select
          required
          value={userId}
          onChange={(event) => setUserId(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm text-neutral-100 outline-none focus:border-neutral-500"
        >
          <option value="" disabled>
            Wybierz…
          </option>
          {roster?.map((member) => (
            <option key={member.id} value={member.id}>
              {member.inGameNickname ?? member.displayName}
            </option>
          ))}
        </select>
      </label>

      <label className="flex flex-col gap-1 text-xs text-neutral-400">
        Rodzaj
        <select
          value={type}
          onChange={(event) => setType(event.target.value as AttendanceIncidentType)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm text-neutral-100 outline-none focus:border-neutral-500"
        >
          <option value="Late">Spóźnienie</option>
          <option value="Absent">Nieobecność</option>
        </select>
      </label>

      <label className="flex flex-col gap-1 text-xs text-neutral-400">
        Data
        <input
          required
          type="date"
          value={occurredOn}
          onChange={(event) => setOccurredOn(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm text-neutral-100 outline-none focus:border-neutral-500"
        />
      </label>

      <label className="flex flex-1 min-w-[10rem] flex-col gap-1 text-xs text-neutral-400">
        Notatka (opcjonalnie)
        <input
          value={note}
          onChange={(event) => setNote(event.target.value)}
          maxLength={300}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm text-neutral-100 outline-none focus:border-neutral-500"
        />
      </label>

      <button
        type="submit"
        disabled={addIncident.isPending || userId === ''}
        className="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {addIncident.isPending ? 'Zapisywanie…' : 'Dodaj'}
      </button>

      {addIncident.isError && <p className="w-full text-sm text-red-400">Nie udało się zapisać wpisu.</p>}
    </form>
  )
}
