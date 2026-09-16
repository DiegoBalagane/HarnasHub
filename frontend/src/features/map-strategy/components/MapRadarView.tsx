import { useState } from 'react'
import type { MapSide } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { mapNames } from '../../nades/labels'
import { useMapPositions } from '../hooks/useMapStrategy'
import { mapSideLabels, mapSides } from '../labels'
import { AddPositionControl } from './AddPositionControl'
import { MapRadar } from './MapRadar'

const editorRoles = new Set(['Coach', 'Manager'])

/** Per-map starting-position board: pick a map and side, then read (or, as Coach/Manager, arrange) the team's setup. */
export function MapRadarView() {
  const [mapName, setMapName] = useState<MapName>('Mirage')
  const [side, setSide] = useState<MapSide>('CT')
  const { data: positions, isLoading, isError } = useMapPositions(mapName, side)
  const role = useAuthStore((state) => state.role)
  const canEdit = role !== null && editorRoles.has(role)

  return (
    <section className="flex w-full max-w-3xl flex-col gap-4">
      <div className="flex flex-wrap items-center gap-3">
        <select
          value={mapName}
          onChange={(event) => setMapName(event.target.value as MapName)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          {mapNames.map((map) => (
            <option key={map} value={map}>
              {map}
            </option>
          ))}
        </select>

        <div className="flex overflow-hidden rounded-md border border-neutral-800">
          {mapSides.map((option) => (
            <button
              key={option}
              type="button"
              onClick={() => setSide(option)}
              className={`px-3 py-2 text-sm ${
                side === option ? 'bg-neutral-100 text-neutral-900' : 'text-neutral-400 hover:text-white'
              }`}
            >
              {mapSideLabels[option]}
            </button>
          ))}
        </div>
      </div>

      {canEdit && (
        <AddPositionControl
          mapName={mapName}
          side={side}
          placedUserIds={(positions ?? []).map((position) => position.userId)}
        />
      )}

      {isLoading && <p className="text-neutral-400">Ładowanie…</p>}
      {isError && <p className="text-red-400">Nie udało się pobrać pozycji na mapie.</p>}

      {positions && (
        <>
          <MapRadar mapName={mapName} side={side} positions={positions} canEdit={canEdit} />

          <p className="text-xs text-neutral-500">
            {positions.length === 0
              ? 'Nikt nie ma jeszcze przypisanej pozycji na tej mapie i stronie.'
              : canEdit
                ? 'Przeciągnij pinezkę, aby zmienić pozycję — zapis następuje po puszczeniu.'
                : 'Najedź na pinezkę, aby zobaczyć zawodnika i jego rolę.'}
          </p>
        </>
      )}
    </section>
  )
}
