import { useState } from 'react'
import type { MapPosition, MapSide } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { mapNames } from '../../nades/labels'
import { useMapPositions } from '../hooks/useMapStrategy'
import { mapSideLabels, mapSides } from '../labels'
import { teamRoleLabels } from '../../roster/labels'
import { AddPositionControl } from './AddPositionControl'
import { MapRadar } from './MapRadar'

/** Per-map starting-position board: pick a map and side, then read (or, as Coach/Manager, arrange) the team's setup. */
export function MapRadarView() {
  const [mapName, setMapName] = useState<MapName>('Mirage')
  const [side, setSide] = useState<MapSide>('CT')
  const { data: positions, isLoading, isError } = useMapPositions(mapName, side)
  const canEdit = useIsCoachOrManager()

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
                ? 'Przeciągnij pinezkę, aby zmienić pozycję, kliknij, aby edytować notatkę.'
                : 'Najedź na pinezkę, aby zobaczyć zawodnika i jego rolę.'}
          </p>

          <PositionNotesList positions={positions} />
        </>
      )}
    </section>
  )
}

/** Readable list of every position's instruction note — the tiny dot on the pin is easy to miss, especially on a phone. */
function PositionNotesList({ positions }: { positions: MapPosition[] }) {
  const withNotes = positions.filter((position) => position.note)

  if (withNotes.length === 0) {
    return null
  }

  return (
    <ul className="flex flex-col gap-2">
      {withNotes.map((position) => (
        <li key={position.id} className="rounded-md border border-neutral-800 px-3 py-2 text-sm">
          <span className="font-medium text-neutral-200">
            {position.inGameNickname ?? position.displayName}
          </span>
          {position.teamRole && (
            <span className="ml-2 text-xs text-neutral-500">({teamRoleLabels[position.teamRole]})</span>
          )}
          <p className="mt-0.5 text-neutral-400">{position.note}</p>
        </li>
      ))}
    </ul>
  )
}
