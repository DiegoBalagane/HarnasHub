import { memo } from 'react'

interface RecordBadgeProps {
  wins: number
  losses: number
  draws: number
}

/** Compact head-to-head record ("3W · 1P · 0R"), or a dash when the team hasn't played them yet. */
export const RecordBadge = memo(function RecordBadge({ wins, losses, draws }: RecordBadgeProps) {
  if (wins + losses + draws === 0) {
    return <span className="text-sm text-neutral-500">brak meczów</span>
  }

  return (
    <span className="text-sm tabular-nums">
      <span className="text-success-400">{wins}W</span>
      <span className="text-neutral-600"> · </span>
      <span className="text-danger-400">{losses}P</span>
      <span className="text-neutral-600"> · </span>
      <span className="text-neutral-400">{draws}R</span>
    </span>
  )
})
