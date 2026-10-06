import { memo } from 'react'
import type { MapName } from '../../../../services/nadesApi'
import type { DemoNade } from '../../../../services/tacticsApi'
import { grenadeTypeColors, grenadeTypeMarks } from '../../../nades/labels'
import { grenadeStrokeColors } from './demoImport'

interface DemoNadeRadarPreviewProps {
  mapName: MapName
  grenades: DemoNade[]
  selectedIds: ReadonlySet<number>
}

/** Radar with a throw → landing line and a type marker per grenade; deselected grenades are dimmed. */
export const DemoNadeRadarPreview = memo(function DemoNadeRadarPreview({
  mapName,
  grenades,
  selectedIds,
}: DemoNadeRadarPreviewProps) {
  return (
    <div className="relative w-full select-none overflow-hidden rounded-md border border-neutral-800 bg-neutral-950">
      <img
        src={`/maps/${mapName.toLowerCase()}.webp`}
        alt={`Radar mapy ${mapName}`}
        draggable={false}
        className="block h-auto w-full"
      />

      <svg viewBox="0 0 100 100" preserveAspectRatio="none" className="pointer-events-none absolute inset-0 h-full w-full">
        {grenades.map((grenade) => (
          <g key={grenade.id} opacity={selectedIds.has(grenade.id) ? 1 : 0.2}>
            <line
              x1={grenade.throwX * 100}
              y1={grenade.throwY * 100}
              x2={grenade.landX * 100}
              y2={grenade.landY * 100}
              stroke={grenadeStrokeColors[grenade.type]}
              strokeWidth={1.5}
              strokeDasharray="3 2"
              vectorEffect="non-scaling-stroke"
            />
            <circle
              cx={grenade.throwX * 100}
              cy={grenade.throwY * 100}
              r={0.6}
              fill={grenadeStrokeColors[grenade.type]}
            />
          </g>
        ))}
      </svg>

      {grenades.map((grenade) => (
        <div
          key={grenade.id}
          title={`${grenade.throwerName} · ${grenade.type}`}
          className={`absolute flex h-5 w-5 -translate-x-1/2 -translate-y-1/2 items-center justify-center rounded-full border border-black/40 text-[10px] font-bold text-neutral-950 shadow ${
            grenadeTypeColors[grenade.type]
          } ${selectedIds.has(grenade.id) ? '' : 'opacity-25'}`}
          style={{ left: `${grenade.landX * 100}%`, top: `${grenade.landY * 100}%` }}
        >
          {grenadeTypeMarks[grenade.type]}
        </div>
      ))}
    </div>
  )
})
