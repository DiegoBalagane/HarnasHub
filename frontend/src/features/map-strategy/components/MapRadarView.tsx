import { useState } from 'react'
import type { MapSide, SidedMapPosition } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { mapNames } from '../../nades/labels'
import { useMapPositions, useMapTextAnnotations } from '../hooks/useMapStrategy'
import { useSideFilter } from '../hooks/useSideFilter'
import { mapSideLabels, mapSides, sideFilterOptions, sideStyles, type SideFilter } from '../labels'
import { pinColorFor } from '../pinColors'
import { pinColorSwatch, teamRoleLabels } from '../../roster/labels'
import { AddPositionControl } from './AddPositionControl'
import { AddTextAnnotationControl } from './AddTextAnnotationControl'
import { MapRadar } from './MapRadar'

interface MapRadarViewProps {
  /** Map chosen by the host page (Playbook); when set, the built-in map selector is hidden. */
  mapName?: MapName
}

/** Per-map starting-position board: one shared radar with both sides' pins; a filter picks which sides are visible and
 * (for Coach/Manager) a segmented control picks the side that new placements are saved to. */
export function MapRadarView({ mapName: controlledMapName }: MapRadarViewProps) {
  const [ownMapName, setMapName] = useState<MapName>('Mirage')
  const mapName = controlledMapName ?? ownMapName
  const [editSide, setEditSide] = useState<MapSide>('CT')
  const [filter, setFilter] = useSideFilter()
  const { data: positions, isLoading, isError } = useMapPositions(mapName)
  const annotations = useMapTextAnnotations(mapName)
  const canEdit = useIsCoachOrManager()

  const visibleSides = filter === 'both' ? mapSides : [filter]
  const visiblePositions = (positions ?? []).filter((position) => visibleSides.includes(position.side))
  const visibleAnnotations = annotations.filter((annotation) => visibleSides.includes(annotation.side))

  function chooseFilter(next: SideFilter) {
    setFilter(next)
    // Placing on a hidden side would look like nothing happened, so the control follows a single-side filter.
    if (next !== 'both') setEditSide(next)
  }

  function chooseEditSide(next: MapSide) {
    setEditSide(next)
    if (filter !== 'both' && filter !== next) setFilter('both')
  }

  return (
    <section className="flex w-full flex-col gap-4">
      <div className="flex flex-wrap items-center gap-3">
        {controlledMapName === undefined && (
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
        )}

        <SegmentedControl
          label="Pokaż"
          options={sideFilterOptions}
          value={filter}
          onChange={chooseFilter}
        />

        {canEdit && (
          <SegmentedControl
            label="Ustawiasz"
            options={mapSides.map((side) => ({ value: side, label: side }))}
            value={editSide}
            onChange={chooseEditSide}
            title={mapSideLabels[editSide]}
          />
        )}
      </div>

      {canEdit && (
        <div className="flex flex-wrap items-center gap-2">
          <AddPositionControl
            mapName={mapName}
            side={editSide}
            placedUserIds={(positions ?? [])
              .filter((position) => position.side === editSide)
              .map((position) => position.userId)}
          />
          <AddTextAnnotationControl mapName={mapName} side={editSide} />
        </div>
      )}

      {isLoading && <p className="text-neutral-400">Ładowanie…</p>}
      {isError && <p className="text-danger-400">Nie udało się pobrać pozycji na mapie.</p>}

      {positions && (
        <>
          <MapRadar
            mapName={mapName}
            positions={visiblePositions}
            annotations={visibleAnnotations}
            canEdit={canEdit}
          />

          <MapLegend positions={visiblePositions} />

          <p className="text-xs text-neutral-500">
            {visiblePositions.length === 0
              ? 'Nikt nie ma jeszcze przypisanej pozycji na tej mapie (w wybranym widoku).'
              : canEdit
                ? 'Przeciągnij pinezkę lub notatkę, aby zmienić pozycję (pinezka zapisuje się na swojej stronie), kliknij, aby edytować. Kółkiem myszy przybliżysz mapę.'
                : 'Najedź na pinezkę, aby zobaczyć zawodnika i jego rolę. Kółkiem myszy przybliżysz mapę.'}
          </p>

          <PositionNotesList positions={visiblePositions} />
        </>
      )}
    </section>
  )
}

interface SegmentedControlProps<T extends string> {
  label: string
  options: { value: T; label: string }[]
  value: T
  onChange: (value: T) => void
  title?: string
}

/** Small labelled toggle group (radio semantics) used for the visibility filter and the editing-side control. */
function SegmentedControl<T extends string>({ label, options, value, onChange, title }: SegmentedControlProps<T>) {
  return (
    <div role="radiogroup" aria-label={label} title={title} className="flex items-center gap-2 text-sm text-neutral-400">
      <span>{label}:</span>
      <div className="flex overflow-hidden rounded-md border border-neutral-800">
        {options.map((option) => (
          <button
            key={option.value}
            type="button"
            role="radio"
            aria-checked={value === option.value}
            onClick={() => onChange(option.value)}
            className={`px-3 py-2 ${
              value === option.value ? 'bg-neutral-100 text-neutral-900' : 'text-neutral-400 hover:text-white'
            }`}
          >
            {option.label}
          </button>
        ))}
      </div>
    </div>
  )
}

/** Legend under the radar: what the side rings mean plus every visible player with their pin colour. */
function MapLegend({ positions }: { positions: SidedMapPosition[] }) {
  const players = new Map<string, SidedMapPosition>()
  for (const position of positions) players.set(position.userId, players.get(position.userId) ?? position)

  return (
    <div aria-label="Legenda" className="flex flex-wrap items-center gap-x-4 gap-y-2 text-xs text-neutral-300">
      {mapSides.map((side) => (
        <span key={side} className="flex items-center gap-1.5">
          <span className={`h-3.5 w-3.5 rounded-full ring-2 ${sideStyles[side].ring}`} />
          {side === 'T' ? 'T — atak' : 'CT — obrona'}
        </span>
      ))}
      {[...players.values()].map((player) => (
        <span key={player.userId} className="flex items-center gap-1.5">
          <span
            className={`h-3 w-3 rounded-full ${player.pinColor ? pinColorSwatch[player.pinColor] : pinColorFor(player.userId)}`}
          />
          {player.inGameNickname ?? player.displayName}
        </span>
      ))}
    </div>
  )
}

/** Readable list of every position's instruction note — the tiny dot on the pin is easy to miss, especially on a phone. */
function PositionNotesList({ positions }: { positions: SidedMapPosition[] }) {
  const withNotes = positions.filter((position) => position.note)

  if (withNotes.length === 0) {
    return null
  }

  return (
    <ul className="flex flex-col gap-2">
      {withNotes.map((position) => (
        <li key={position.id} className="rounded-md border border-neutral-800 px-3 py-2 text-sm">
          <span className={`mr-2 text-xs font-bold ${sideStyles[position.side].text}`}>{position.side}</span>
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
