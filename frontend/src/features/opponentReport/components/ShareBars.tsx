import { memo } from 'react'
import type { TendencyShare } from '../../../services/opponentDemosApi'

interface ShareBarsProps {
  title: string
  shares: TendencyShare[]
  /** Optional display label per share label (e.g. Polish names of buy types). */
  labels?: Record<string, string>
}

/** A small horizontal bar chart of a distribution. */
export const ShareBars = memo(function ShareBars({ title, shares, labels }: ShareBarsProps) {
  return (
    <div className="flex flex-col gap-1">
      <span className="text-xs uppercase tracking-wide text-neutral-500">{title}</span>
      {shares.length === 0 && <span className="text-xs text-neutral-500">brak danych</span>}
      {shares.map((share) => (
        <div key={share.label} className="flex items-center gap-2 text-xs">
          <span className="w-24 shrink-0 truncate text-neutral-300">
            {labels?.[share.label] ?? share.label}
          </span>
          <div className="h-2 flex-1 rounded bg-neutral-800">
            <div className="h-2 rounded bg-primary-500" style={{ width: `${share.percent}%` }} />
          </div>
          <span className="w-16 shrink-0 text-right text-neutral-400">
            {Math.round(share.percent)}% ({share.count})
          </span>
        </div>
      ))}
    </div>
  )
})
