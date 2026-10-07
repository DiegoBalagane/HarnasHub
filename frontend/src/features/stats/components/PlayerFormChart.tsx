import { memo, useMemo, useState } from 'react'
import type { AdvancedFormPoint, AdvancedPlayer } from '../../../services/advancedStatsApi'
import { buildFormSeries } from '../advancedStats'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

const WIDTH = 600
const HEIGHT = 220
const PAD_LEFT = 36
const PAD_RIGHT = 16
const PAD_TOP = 16
const PAD_BOTTOM = 28
const PLOT_WIDTH = WIDTH - PAD_LEFT - PAD_RIGHT
const PLOT_HEIGHT = HEIGHT - PAD_TOP - PAD_BOTTOM
const DEFAULT_SELECTED = 3
const COLORS = ['#f59e0b', '#38bdf8', '#34d399', '#f472b6', '#a78bfa', '#fb923c', '#facc15', '#94a3b8']

interface PlayerFormChartProps {
  players: AdvancedPlayer[]
  form: AdvancedFormPoint[]
}

/** Line chart of the rating per match for the selected players (multi-select), same visual style as the team trend chart. */
function PlayerFormChartInner({ players, form }: PlayerFormChartProps) {
  const [selection, setSelection] = useState<string[] | null>(null)
  const [hover, setHover] = useState<AdvancedFormPoint | null>(null)
  const selected = useMemo(
    () => selection ?? players.slice(0, DEFAULT_SELECTED).map((p) => p.userId),
    [selection, players],
  )

  const { matchIds, series, min, max } = useMemo(() => {
    const model = buildFormSeries(form, selected)
    const ratings = model.series.flatMap((s) => s.points.map((p) => p.point.rating))
    return {
      ...model,
      min: Math.floor(Math.min(0.5, ...ratings) * 10) / 10,
      max: Math.ceil(Math.max(1.5, ...ratings) * 10) / 10,
    }
  }, [form, selected])

  const xOf = (index: number) =>
    PAD_LEFT + (matchIds.length > 1 ? (PLOT_WIDTH / (matchIds.length - 1)) * index : PLOT_WIDTH / 2)
  const yOf = (rating: number) => PAD_TOP + PLOT_HEIGHT * (1 - (rating - min) / (max - min))
  const ticks = [min, (min + max) / 2, max]

  function toggle(userId: string) {
    setSelection(selected.includes(userId) ? selected.filter((id) => id !== userId) : [...selected, userId])
  }

  return (
    <div className="w-full">
      <div className="mb-2 flex flex-wrap gap-2" role="group" aria-label="Gracze na wykresie formy">
        {players.map((player, index) => {
          const active = selected.includes(player.userId)
          return (
            <button
              key={player.userId}
              type="button"
              aria-pressed={active}
              onClick={() => toggle(player.userId)}
              className={`rounded-md border px-2 py-1 text-xs transition ${
                active ? 'border-neutral-500 text-neutral-100' : 'border-neutral-800 text-neutral-500 hover:text-neutral-300'
              }`}
            >
              <span
                className="mr-1 inline-block h-2 w-2 rounded-full"
                style={{ backgroundColor: COLORS[index % COLORS.length] }}
              />
              {player.name}
            </button>
          )
        })}
      </div>

      {matchIds.length === 0 ? (
        <p className="text-neutral-400">Wybierz zawodnika, żeby zobaczyć jego formę.</p>
      ) : (
        <svg
          viewBox={`0 0 ${WIDTH} ${HEIGHT}`}
          className="w-full"
          onMouseLeave={() => setHover(null)}
          role="img"
          aria-label="Rating w kolejnych meczach"
        >
          {ticks.map((tick) => (
            <g key={tick}>
              <line x1={PAD_LEFT} y1={yOf(tick)} x2={WIDTH - PAD_RIGHT} y2={yOf(tick)} stroke="#27272a" strokeWidth={1} />
              <text x={PAD_LEFT - 8} y={yOf(tick) + 3} textAnchor="end" fontSize={10} fill="#71717a">
                {tick.toFixed(2)}
              </text>
            </g>
          ))}
          {series.map((s) => {
            const color = COLORS[players.findIndex((p) => p.userId === s.userId) % COLORS.length]
            const path = s.points
              .map((p, i) => `${i === 0 ? 'M' : 'L'} ${xOf(p.index)} ${yOf(p.point.rating)}`)
              .join(' ')
            return (
              <g key={s.userId}>
                <path d={path} fill="none" stroke={color} strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" />
                {s.points.map((p) => (
                  <circle
                    key={p.point.matchResultId}
                    cx={xOf(p.index)}
                    cy={yOf(p.point.rating)}
                    r={4}
                    fill={color}
                    onMouseEnter={() => setHover(p.point)}
                  />
                ))}
              </g>
            )
          })}
        </svg>
      )}

      {hover && (
        <div className="mt-2 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-xs text-neutral-300">
          <p className="text-neutral-400">
            {dateFormatter.format(new Date(hover.playedAtUtc))} · {hover.opponent} · {hover.map}
          </p>
          <p>
            {players.find((p) => p.userId === hover.userId)?.name}: rating {hover.rating.toFixed(2)} · ADR{' '}
            {hover.adr.toFixed(1)}
          </p>
        </div>
      )}
    </div>
  )
}

export const PlayerFormChart = memo(PlayerFormChartInner)
