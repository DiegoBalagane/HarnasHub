import { memo, useState } from 'react'
import type { OpeningDuel, Side } from '../../../services/matchAnalysisApi'

interface OpeningDuelsMapProps {
  mapName: string
  duels: OpeningDuel[]
}

type SideFilter = Side | ''

/** Mini-radar of our opening duels: a dot per duel at the victim's spot, green when we won it and red when we lost it. */
export const OpeningDuelsMap = memo(function OpeningDuelsMap({ mapName, duels }: OpeningDuelsMapProps) {
  const [side, setSide] = useState<SideFilter>('')
  const positioned = duels.filter((duel) => duel.victimX !== null && duel.victimY !== null)
  const visible = positioned.filter((duel) => side === '' || duel.ourSide === side)

  if (positioned.length === 0) {
    return null
  }

  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-center gap-1 text-xs">
        {(['', 'T', 'CT'] as const).map((value) => (
          <button
            key={value || 'all'}
            type="button"
            onClick={() => setSide(value)}
            className={`rounded px-2 py-1 transition ${
              side === value ? 'bg-neutral-700 text-neutral-100' : 'text-neutral-400 hover:text-neutral-200'
            }`}
          >
            {value === '' ? 'Obie strony' : value}
          </button>
        ))}
      </div>
      <div className="relative w-full max-w-md select-none overflow-hidden rounded-md border border-neutral-800 bg-neutral-950">
        <img
          src={`/maps/${mapName.toLowerCase()}.webp`}
          alt={`Radar mapy ${mapName}`}
          draggable={false}
          className="block h-auto w-full"
        />
        {visible.map((duel) => (
          <div
            key={`${duel.roundNumber}-${duel.victimName}`}
            title={`R${duel.roundNumber}: ${duel.killerName ?? '?'} → ${duel.victimName}${duel.zone ? ` (${duel.zone})` : ''}`}
            className={`absolute h-3 w-3 -translate-x-1/2 -translate-y-1/2 rounded-full border border-black/50 opacity-80 ${
              duel.wonByUs ? 'bg-success-500' : 'bg-danger-500'
            }`}
            style={{ left: `${(duel.victimX ?? 0) * 100}%`, top: `${(duel.victimY ?? 0) * 100}%` }}
          />
        ))}
      </div>
    </div>
  )
})
