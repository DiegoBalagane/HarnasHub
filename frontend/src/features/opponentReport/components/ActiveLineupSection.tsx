import { memo } from 'react'
import type { ActiveLineup } from '../../../services/opponentReportApi'
import { inactiveSummary, inactiveTitle, lineupHeading, lineupPlayerLabel } from '../lineup'

/** Report header line: the lineup the numbers are based on (current ESEA season when known), why, and the other team members collapsed below. */
export const ActiveLineupSection = memo(function ActiveLineupSection({ lineup }: { lineup: ActiveLineup }) {
  return (
    <div className="flex flex-col gap-1 text-sm text-neutral-400">
      <p>
        <span className="text-neutral-300">{lineupHeading(lineup)}:</span>{' '}
        {lineup.active.map((player) => lineupPlayerLabel(player, lineup.windowGames)).join(', ') || '—'}
      </p>
      <p className="text-xs text-neutral-500">
        {lineup.seasonCompetition ? `${lineup.seasonCompetition} · ` : ''}
        {lineup.basis}
      </p>
      {lineup.inactive.length > 0 && (
        <details className="text-xs">
          <summary className="cursor-pointer select-none text-neutral-400 hover:text-white">
            {inactiveSummary(lineup)}
          </summary>
          <ul className="mt-1 flex flex-wrap gap-2">
            {lineup.inactive.map((player) => (
              <li
                key={player.playerId}
                title={inactiveTitle(player, lineup)}
                className="rounded-full border border-neutral-800 px-2 py-0.5 text-neutral-500"
              >
                {player.nickname}
              </li>
            ))}
          </ul>
        </details>
      )}
    </div>
  )
})
