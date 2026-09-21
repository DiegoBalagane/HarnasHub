import { useState } from 'react'
import type { DeathPosition } from '../../../services/statsApi'

interface DeathMapPlayer {
  key: string
  name: string
  deathPositions: DeathPosition[]
}

interface DeathMapViewProps {
  mapName: string
  players: DeathMapPlayer[]
}

const sideColor: Record<DeathPosition['side'], string> = {
  CT: 'bg-blue-500',
  T: 'bg-orange-500',
}

type Filter = '' | 'CT' | 'T' | string

/** Shows where every player died on the match's map, colour-coded by side (CT/T); narrow it down to one side or one
 * player to isolate a pattern (e.g. repeated bad CT positions, or just one player's deaths). */
export function DeathMapView({ mapName, players }: DeathMapViewProps) {
  const [filter, setFilter] = useState<Filter>('')

  const playersWithDeaths = players.filter((player) => player.deathPositions.length > 0)
  const isPlayerFilter = filter !== '' && filter !== 'CT' && filter !== 'T'

  const visibleDeaths = playersWithDeaths.flatMap((player) =>
    player.deathPositions
      .filter((death) => {
        if (filter === '') return true
        if (filter === 'CT' || filter === 'T') return death.side === filter
        return player.key === filter
      })
      .map((death, index) => ({ key: `${player.key}-${index}`, name: player.name, death })),
  )

  if (playersWithDeaths.length === 0) {
    return null
  }

  return (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-800 p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="text-sm font-medium">Mapa śmierci — {mapName}</h3>
        <div className="flex items-center gap-3 text-xs">
          <span className="flex items-center gap-1 text-neutral-400">
            <span className="h-2 w-2 rounded-full bg-blue-500" /> CT
          </span>
          <span className="flex items-center gap-1 text-neutral-400">
            <span className="h-2 w-2 rounded-full bg-orange-500" /> T
          </span>
          <select
            value={filter}
            onChange={(event) => setFilter(event.target.value)}
            className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500"
          >
            <option value="">Wszyscy gracze</option>
            <option value="CT">Tylko CT</option>
            <option value="T">Tylko T</option>
            {playersWithDeaths.map((player) => (
              <option key={player.key} value={player.key}>
                {player.name} ({player.deathPositions.length})
              </option>
            ))}
          </select>
        </div>
      </div>

      <div className="relative w-full max-w-md select-none overflow-hidden rounded-md border border-neutral-800 bg-neutral-950">
        <img
          src={`/maps/${mapName.toLowerCase()}.webp`}
          alt={`Radar mapy ${mapName}`}
          draggable={false}
          className="block h-auto w-full"
        />
        {visibleDeaths.map(({ key, name, death }) => (
          <div
            key={key}
            title={name}
            className={`absolute h-2.5 w-2.5 -translate-x-1/2 -translate-y-1/2 rounded-full border border-black/40 ${sideColor[death.side]} ${
              filter ? 'opacity-90' : 'opacity-60'
            }`}
            style={{ left: `${death.x * 100}%`, top: `${death.y * 100}%` }}
          />
        ))}
      </div>

      <p className="text-xs text-neutral-500">
        {isPlayerFilter
          ? 'Zgony wybranego gracza — pomaga zauważyć powtarzające się złe pozycje.'
          : filter === 'CT' || filter === 'T'
            ? `Zgony wszystkich graczy po stronie ${filter} — pomaga zauważyć złe pozycje typowe dla tej strony.`
            : 'Zgony wszystkich graczy naraz — wybierz stronę albo gracza z listy, żeby zawęzić widok.'}
      </p>
    </div>
  )
}
