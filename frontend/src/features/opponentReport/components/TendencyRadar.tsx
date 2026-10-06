import { memo, useState } from 'react'
import type { MapTendencies } from '../../../services/opponentDemosApi'
import { grenadeColors } from '../tendencyLabels'

interface TendencyRadarProps {
  tendencies: MapTendencies
}

type RadarLayer = 'ct' | 't' | 'nades'

const layerLabels: Record<RadarLayer, string> = {
  ct: 'Ustawienie CT (0:20)',
  t: 'Wejścia T',
  nades: 'Granaty T',
}

/** Coordinates are radar fractions; the SVG uses a 0–100 box stretched over the radar image. */
const toBox = (fraction: number) => fraction * 100

/** Mini radar of one map's tendencies: CT setup heatmap dots (AWP in purple), T entry arrows from spawn (width by share)
 * and T grenade clusters (circle per cluster, coloured by type) — one layer at a time. */
export const TendencyRadar = memo(function TendencyRadar({ tendencies }: TendencyRadarProps) {
  const [layer, setLayer] = useState<RadarLayer>('ct')
  const { ct, t } = tendencies
  const spawn =
    tendencies.tSpawnX !== null && tendencies.tSpawnY !== null
      ? { x: tendencies.tSpawnX, y: tendencies.tSpawnY }
      : null

  return (
    <div className="flex flex-col gap-2">
      <div className="flex gap-1">
        {(Object.keys(layerLabels) as RadarLayer[]).map((key) => (
          <button
            key={key}
            type="button"
            onClick={() => setLayer(key)}
            className={`rounded px-2 py-0.5 text-xs ${layer === key ? 'bg-primary-500 text-primary-950' : 'bg-neutral-800 text-neutral-300'}`}
          >
            {layerLabels[key]}
          </button>
        ))}
      </div>
      <div className="relative w-full max-w-sm">
        <img
          src={`/maps/${tendencies.mapName.toLowerCase()}.webp`}
          alt={tendencies.mapName}
          className="w-full rounded-md opacity-80"
        />
        <svg viewBox="0 0 100 100" preserveAspectRatio="none" className="absolute inset-0 h-full w-full">
          <defs>
            <marker
              id={`arrow-${tendencies.mapName}`}
              viewBox="0 0 10 10"
              refX="8"
              refY="5"
              markerWidth="4"
              markerHeight="4"
              orient="auto"
            >
              <path d="M0,0 L10,5 L0,10 z" fill="#f97316" />
            </marker>
          </defs>

          {layer === 'ct' &&
            [...ct.setupPositions, ...ct.awpKillPositions.map((p) => ({ ...p, awp: true }))].map(
              (point, i) => (
                <circle
                  key={i}
                  cx={toBox(point.x)}
                  cy={toBox(point.y)}
                  r={0.9}
                  fill={point.awp ? '#a855f7' : '#38bdf8'}
                  fillOpacity={0.45}
                />
              ),
            )}

          {layer === 't' &&
            spawn &&
            t.entries.map((entry) => (
              <line
                key={entry.area}
                x1={toBox(spawn.x)}
                y1={toBox(spawn.y)}
                x2={toBox(entry.x)}
                y2={toBox(entry.y)}
                stroke="#f97316"
                strokeWidth={1 + entry.percent / 20}
                strokeOpacity={0.85}
                markerEnd={`url(#arrow-${tendencies.mapName})`}
              />
            ))}

          {layer === 'nades' &&
            t.grenadeClusters.map((cluster, i) => (
              <circle
                key={i}
                cx={toBox(cluster.x)}
                cy={toBox(cluster.y)}
                r={2}
                fill={grenadeColors[cluster.type]}
                fillOpacity={0.25 + cluster.perRoundPercent / 200}
                stroke={grenadeColors[cluster.type]}
                strokeWidth={0.3}
              />
            ))}
        </svg>
      </div>
      {layer === 't' && !spawn && (
        <span className="text-xs text-neutral-500">Ta mapa nie ma jeszcze stref — brak strzałek wejść.</span>
      )}
    </div>
  )
})
