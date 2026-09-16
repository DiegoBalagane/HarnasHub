import { useState } from 'react'
import type { MapSide } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import { useRoster } from '../../roster/hooks/useRoster'
import { useSetPlayerPosition } from '../hooks/useMapStrategy'

interface AddPositionControlProps {
  mapName: MapName
  side: MapSide
  /** Ids already on the radar — those members are hidden from the picker. */
  placedUserIds: string[]
}

/** Coach/Manager picker that drops a new pin in the middle of the radar, ready to be dragged into place. */
export function AddPositionControl({ mapName, side, placedUserIds }: AddPositionControlProps) {
  const [userId, setUserId] = useState('')
  const { data: roster } = useRoster()
  const setPlayerPosition = useSetPlayerPosition()

  const selectable = (roster ?? []).filter(
    (member) => member.role !== 'Guest' && !placedUserIds.includes(member.id),
  )

  function handleAdd() {
    if (!userId) {
      return
    }

    setPlayerPosition.mutate(
      { mapName, side, userId, label: null, x: 0.5, y: 0.5, note: null },
      { onSuccess: () => setUserId('') },
    )
  }

  return (
    <div className="flex items-center gap-2">
      <select
        value={userId}
        onChange={(event) => setUserId(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      >
        <option value="">Wybierz zawodnika…</option>
        {selectable.map((member) => (
          <option key={member.id} value={member.id}>
            {member.inGameNickname ?? member.displayName}
          </option>
        ))}
      </select>

      <button
        type="button"
        onClick={handleAdd}
        disabled={!userId || setPlayerPosition.isPending}
        className="rounded-md bg-neutral-100 px-3 py-2 text-sm font-medium text-neutral-900 disabled:opacity-40"
      >
        Dodaj pozycję
      </button>

      {setPlayerPosition.isError && (
        <span className="text-xs text-red-400">Nie udało się zapisać pozycji.</span>
      )}
    </div>
  )
}
