import { useState } from 'react'
import type { MapSide } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import type { EconomyType } from '../../../services/tacticsApi'
import { mapNames } from '../../nades/labels'
import { mapSideLabels, mapSides } from '../../map-strategy/labels'
import { useTactics } from '../hooks/useTactics'
import { economyLabels, economyTypes } from '../labels'

interface TacticListProps {
  onSelect: (tacticId: string) => void
}

/** Filterable grid of saved tactics — pick a map, side and/or economy, click a card to open its radar. */
export function TacticList({ onSelect }: TacticListProps) {
  const [mapName, setMapName] = useState<MapName | ''>('')
  const [side, setSide] = useState<MapSide | ''>('')
  const [economy, setEconomy] = useState<EconomyType | ''>('')
  const {
    data: tactics,
    isLoading,
    isError,
  } = useTactics({ mapName: mapName || undefined, side: side || undefined, economy: economy || undefined })

  return (
    <div className="flex w-full max-w-3xl flex-col gap-4">
      <div className="flex flex-wrap gap-3">
        <select
          value={mapName}
          onChange={(event) => setMapName(event.target.value as MapName | '')}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          <option value="">Wszystkie mapy</option>
          {mapNames.map((map) => (
            <option key={map} value={map}>
              {map}
            </option>
          ))}
        </select>

        <div className="flex overflow-hidden rounded-md border border-neutral-800">
          <button
            type="button"
            onClick={() => setSide('')}
            className={`px-3 py-2 text-sm ${side === '' ? 'bg-neutral-100 text-neutral-900' : 'text-neutral-400 hover:text-white'}`}
          >
            Obie strony
          </button>
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

        <select
          value={economy}
          onChange={(event) => setEconomy(event.target.value as EconomyType | '')}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          <option value="">Każda ekonomia</option>
          {economyTypes.map((type) => (
            <option key={type} value={type}>
              {economyLabels[type]}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <p className="text-neutral-400">Ładowanie…</p>}
      {isError && <p className="text-red-400">Nie udało się pobrać taktyk.</p>}
      {tactics?.length === 0 && <p className="text-neutral-400">Brak taktyk spełniających filtry.</p>}

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        {tactics?.map((tactic) => (
          <button
            key={tactic.id}
            type="button"
            onClick={() => onSelect(tactic.id)}
            className="flex flex-col gap-1 rounded-md border border-neutral-800 p-4 text-left transition hover:border-neutral-500"
          >
            <div className="flex items-center justify-between">
              <p className="font-medium">{tactic.name}</p>
              <span className="rounded bg-neutral-800 px-2 py-0.5 text-xs text-neutral-300">
                {economyLabels[tactic.economy]}
              </span>
            </div>
            <p className="text-sm text-neutral-400">
              {tactic.mapName} · {mapSideLabels[tactic.side]} · {tactic.pointCount}{' '}
              {tactic.pointCount === 1 ? 'punkt' : 'punktów'}
            </p>
            {tactic.note && <p className="mt-1 line-clamp-2 text-sm text-neutral-500">{tactic.note}</p>}
          </button>
        ))}
      </div>
    </div>
  )
}
