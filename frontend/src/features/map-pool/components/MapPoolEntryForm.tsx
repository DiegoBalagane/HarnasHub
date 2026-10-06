import { useState } from 'react'
import type { MapPoolMap, MapPoolStatus } from '../../../services/mapPoolApi'
import { useSetMapPoolEntry } from '../hooks/useMapPool'
import { mapPoolStatusLabels, mapPoolStatuses } from '../labels'

const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

interface MapPoolEntryFormProps {
  map: MapPoolMap
  /** Called after a successful save — e.g. to close the hosting modal. */
  onDone: () => void
}

/** Coach/Manager form for a map's pool status and a short remark; an empty status clears the classification. */
export function MapPoolEntryForm({ map, onDone }: MapPoolEntryFormProps) {
  const [status, setStatus] = useState<MapPoolStatus | ''>(map.status ?? '')
  const [note, setNote] = useState(map.note ?? '')
  const setEntry = useSetMapPoolEntry()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setEntry.mutate(
      { mapName: map.mapName, payload: { status: status || null, note: status ? note : undefined } },
      { onSuccess: onDone },
    )
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-3">
      <select
        value={status}
        onChange={(event) => setStatus(event.target.value as MapPoolStatus | '')}
        className={inputClass}
      >
        <option value="">Nieustalony</option>
        {mapPoolStatuses.map((option) => (
          <option key={option} value={option}>
            {mapPoolStatusLabels[option]}
          </option>
        ))}
      </select>

      <input
        maxLength={300}
        disabled={!status}
        placeholder="Krótka uwaga (opcjonalnie), np. „słaby CT, do dopracowania retake B”"
        value={note}
        onChange={(event) => setNote(event.target.value)}
        className={`${inputClass} disabled:opacity-50`}
      />

      {setEntry.isError && <p className="text-sm text-danger-400">Nie udało się zapisać statusu mapy.</p>}

      <button
        type="submit"
        disabled={setEntry.isPending}
        className="self-start rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
      >
        {setEntry.isPending ? 'Zapisywanie…' : 'Zapisz'}
      </button>
    </form>
  )
}
