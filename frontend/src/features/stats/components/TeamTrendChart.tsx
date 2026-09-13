import { useMemo, useState } from 'react'
import { useTeamTrend } from '../hooks/useStats'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

const WIDTH = 600
const HEIGHT = 220
const PAD_LEFT = 36
const PAD_RIGHT = 16
const PAD_TOP = 16
const PAD_BOTTOM = 28
const PLOT_WIDTH = WIDTH - PAD_LEFT - PAD_RIGHT
const PLOT_HEIGHT = HEIGHT - PAD_TOP - PAD_BOTTOM

/** Line chart of the team's cumulative win rate over logged matches, with a hover crosshair. */
export function TeamTrendChart() {
  const { data: points, isLoading, isError } = useTeamTrend()
  const [hoverIndex, setHoverIndex] = useState<number | null>(null)

  const scaled = useMemo(() => {
    if (!points || points.length === 0) return []
    const step = points.length > 1 ? PLOT_WIDTH / (points.length - 1) : 0
    return points.map((point, index) => ({
      ...point,
      x: PAD_LEFT + step * index,
      y: PAD_TOP + PLOT_HEIGHT * (1 - point.winRatePercentage / 100),
    }))
  }, [points])

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie trendu…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać trendu.</p>
  }

  if (!points || points.length === 0) {
    return <p className="text-neutral-400">Brak danych — dodaj pierwszy wynik, żeby zobaczyć trend.</p>
  }

  const linePath = scaled.map((point, index) => `${index === 0 ? 'M' : 'L'} ${point.x} ${point.y}`).join(' ')
  const areaPath = `${linePath} L ${scaled[scaled.length - 1].x} ${PAD_TOP + PLOT_HEIGHT} L ${scaled[0].x} ${PAD_TOP + PLOT_HEIGHT} Z`
  const last = scaled[scaled.length - 1]
  const hovered = hoverIndex !== null ? scaled[hoverIndex] : null

  function handleMouseMove(event: React.MouseEvent<SVGSVGElement>) {
    const rect = event.currentTarget.getBoundingClientRect()
    const relativeX = ((event.clientX - rect.left) / rect.width) * WIDTH
    const step = scaled.length > 1 ? PLOT_WIDTH / (scaled.length - 1) : 1
    const index = Math.max(0, Math.min(scaled.length - 1, Math.round((relativeX - PAD_LEFT) / step)))
    setHoverIndex(index)
  }

  return (
    <div className="w-full max-w-2xl">
      <p className="mb-1 text-sm text-neutral-400">
        Skuteczność drużyny w czasie ·{' '}
        <span className="font-medium text-neutral-200">{last.winRatePercentage.toFixed(0)}%</span> (
        {last.cumulativeWins}W / {last.cumulativeLosses}L)
      </p>

      <svg
        viewBox={`0 0 ${WIDTH} ${HEIGHT}`}
        className="w-full"
        onMouseMove={handleMouseMove}
        onMouseLeave={() => setHoverIndex(null)}
      >
        {[0, 25, 50, 75, 100].map((tick) => {
          const y = PAD_TOP + PLOT_HEIGHT * (1 - tick / 100)
          return (
            <g key={tick}>
              <line x1={PAD_LEFT} y1={y} x2={WIDTH - PAD_RIGHT} y2={y} stroke="#27272a" strokeWidth={1} />
              <text x={PAD_LEFT - 8} y={y + 3} textAnchor="end" fontSize={10} fill="#71717a">
                {tick}%
              </text>
            </g>
          )
        })}

        <path d={areaPath} fill="#ef4444" fillOpacity={0.12} stroke="none" />
        <path
          d={linePath}
          fill="none"
          stroke="#ef4444"
          strokeWidth={2}
          strokeLinecap="round"
          strokeLinejoin="round"
        />

        <circle cx={last.x} cy={last.y} r={3.5} fill="#ef4444" />

        {hovered && (
          <>
            <line
              x1={hovered.x}
              y1={PAD_TOP}
              x2={hovered.x}
              y2={PAD_TOP + PLOT_HEIGHT}
              stroke="#52525b"
              strokeWidth={1}
            />
            <circle cx={hovered.x} cy={hovered.y} r={4} fill="#fafafa" stroke="#ef4444" strokeWidth={2} />
          </>
        )}
      </svg>

      {hovered && (
        <div className="mt-2 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-xs text-neutral-300">
          <p className="text-neutral-400">{dateFormatter.format(new Date(hovered.playedAtUtc))}</p>
          <p>
            {hovered.won ? (
              <span className="text-green-400">Wygrana</span>
            ) : (
              <span className="text-red-400">Przegrana</span>
            )}{' '}
            · {hovered.winRatePercentage.toFixed(0)}% skuteczności ({hovered.cumulativeWins}W /{' '}
            {hovered.cumulativeLosses}L)
          </p>
        </div>
      )}
    </div>
  )
}
