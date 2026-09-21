import { useState } from 'react'
import type { DeathPosition, MapSide } from '../../../services/statsApi'

interface DeathMapPlayer {
  key: string
  name: string
  deathPositions: DeathPosition[]
}

interface DeathMapViewProps {
  mapName: string
  players: DeathMapPlayer[]
}

const sideColor: Record<MapSide, string> = {
  CT: 'bg-blue-500',
  T: 'bg-orange-500',
}

type SideFilter = MapSide | ''

/** Shows where every player died on the match's map, colour-coded by side (CT/T). The player and side pickers
 * combine (AND) — e.g. one player + "T" shows only that player's T-side deaths — so a coach can isolate exactly
 * "this player's bad CT positions" instead of only per-player or only per-side. */
export function DeathMapView({ mapName, players }: DeathMapViewProps) {
  const [playerFilter, setPlayerFilter] = useState('')
  const [sideFilter, setSideFilter] = useState<SideFilter>('')

  const playersWithDeaths = players.filter((player) => player.deathPositions.length > 0)

  const visibleDeaths = playersWithDeaths
    .filter((player) => playerFilter === '' || player.key === playerFilter)
    .flatMap((player) =>
      player.deathPositions
        .filter((death) => sideFilter === '' || death.side === sideFilter)
        .map((death, index) => ({ key: `${player.key}-${index}`, name: player.name, death })),
    )

  if (playersWithDeaths.length === 0) {
    return null
  }

  const isNarrowed = playerFilter !== '' || sideFilter !== ''

  return (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-800 p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="text-sm font-medium">Mapa śmierci — {mapName}</h3>
        <div className="flex flex-wrap items-center gap-2 text-xs">
          <div className="flex items-center gap-1 rounded-md border border-neutral-800 p-0.5">
            {(['', 'CT', 'T'] as const).map((side) => (
              <button
                key={side || 'all'}
                type="button"
                onClick={() => setSideFilter(side)}
                className={`flex items-center gap-1 rounded px-2 py-1 transition ${
                  sideFilter === side ? 'bg-neutral-700 text-neutral-100' : 'text-neutral-400 hover:text-neutral-200'
                }`}
              >
                {side !== '' && <span className={`h-2 w-2 rounded-full ${sideColor[side]}`} />}
                {side === '' ? 'Obie strony' : side}
              </button>
            ))}
          </div>
          <select
            value={playerFilter}
            onChange={(event) => setPlayerFilter(event.target.value)}
            className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500"
          >
            <option value="">Wszyscy gracze</option>
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
              isNarrowed ? 'opacity-90' : 'opacity-60'
            }`}
            style={{ left: `${death.x * 100}%`, top: `${death.y * 100}%` }}
          />
        ))}
      </div>

      <p className="text-xs text-neutral-500">
        {isNarrowed
          ? 'Zawężony widok — pomaga zauważyć powtarzające się złe pozycje.'
          : 'Zgony wszystkich graczy naraz — wybierz stronę i/albo gracza z listy, żeby zawęzić widok.'}
      </p>
    </div>
  )
}
