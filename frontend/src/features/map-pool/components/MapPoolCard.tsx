import { memo } from 'react'
import { Link } from 'react-router-dom'
import type { MapPoolMap } from '../../../services/mapPoolApi'
import { formDotClasses, mapPoolStatusClasses, mapPoolStatusLabels } from '../labels'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })
const formLabels = { W: 'wygrana', L: 'przegrana', D: 'remis' } as const

interface MapPoolCardProps {
  map: MapPoolMap
  canManage: boolean
  onEdit: (map: MapPoolMap) => void
}

/** One map of the pool: radar thumbnail, status badge, record with win-rate bar, recent form and a tactics shortcut. */
export const MapPoolCard = memo(function MapPoolCard({ map, canManage, onEdit }: MapPoolCardProps) {
  const played = map.wins + map.losses + map.draws
  const isBanned = map.status === 'Ban'

  return (
    <li
      className={`flex flex-col overflow-hidden rounded-md border border-neutral-800 ${isBanned ? 'opacity-60' : ''}`}
    >
      <div className="relative h-24 bg-neutral-900">
        <img
          src={`/maps/${map.mapName.toLowerCase()}.webp`}
          alt=""
          className="h-full w-full object-cover opacity-40"
          loading="lazy"
        />
        <div className="absolute inset-0 flex items-end justify-between gap-2 p-3">
          <h2 className="text-lg font-semibold drop-shadow">{map.mapName}</h2>
          <span
            className={`rounded-full border px-2 py-0.5 text-xs ${
              map.status
                ? mapPoolStatusClasses[map.status]
                : 'border-neutral-700 bg-neutral-900/70 text-neutral-400'
            }`}
          >
            {map.status ? mapPoolStatusLabels[map.status] : 'Nieustalony'}
          </span>
        </div>
      </div>

      <div className="flex flex-1 flex-col gap-2 p-3">
        {played === 0 ? (
          <p className="text-sm text-neutral-500">Brak rozegranych meczów.</p>
        ) : (
          <>
            <div className="flex items-baseline justify-between text-sm">
              <span className="tabular-nums">
                <span className="text-success-400">{map.wins}W</span>
                <span className="text-neutral-600"> · </span>
                <span className="text-danger-400">{map.losses}P</span>
                <span className="text-neutral-600"> · </span>
                <span className="text-neutral-400">{map.draws}R</span>
              </span>
              <span className="font-medium tabular-nums">{map.winRatePercentage}%</span>
            </div>
            <div className="h-1.5 overflow-hidden rounded-full bg-neutral-800" aria-hidden>
              <div className="h-full bg-success-500" style={{ width: `${map.winRatePercentage ?? 0}%` }} />
            </div>
            <div className="flex items-center justify-between text-xs text-neutral-500">
              <span className="flex items-center gap-1" title="Ostatnie mecze, najnowszy z lewej">
                Forma:
                {map.recentForm.map((outcome, index) => (
                  <span
                    key={index}
                    title={formLabels[outcome]}
                    className={`inline-block h-2.5 w-2.5 rounded-full ${formDotClasses[outcome]}`}
                  />
                ))}
              </span>
              {map.lastPlayedAtUtc && <span>{dateFormatter.format(new Date(map.lastPlayedAtUtc))}</span>}
            </div>
          </>
        )}

        {map.note && <p className="text-sm text-neutral-300">{map.note}</p>}

        <div className="mt-auto flex items-center justify-between pt-1 text-sm">
          <Link to={`/playbook?tab=tactics&map=${map.mapName}`} className="text-neutral-400 hover:text-white">
            Taktyki ({map.tacticCount}) →
          </Link>
          {canManage && (
            <button
              type="button"
              onClick={() => onEdit(map)}
              className="rounded-md px-2 py-1 text-neutral-500 transition hover:bg-neutral-800 hover:text-neutral-200"
            >
              ✎ Status
            </button>
          )}
        </div>
      </div>
    </li>
  )
})
