import { memo } from 'react'
import type { MatchRound, RoundKill, TeamEconomy } from '../../../services/matchAnalysisApi'
import type { RoundTacticMatch } from '../../../services/tacticMatchingApi'
import { buyTypeLabels, buyTypeShort, endReasonLabels, formatRoundTime } from '../labels'

interface RoundRowProps {
  round: MatchRound
  expanded: boolean
  onToggle: (roundNumber: number) => void
  /** Opens the 2D replay of this round. */
  onReplay: (roundNumber: number) => void
  /** The Playbook tactic this round was automatically matched to, if any. */
  tacticMatch?: RoundTacticMatch
}

function BuyBadge({ economy }: { economy: TeamEconomy | null }) {
  if (!economy) {
    return <span className="w-8 text-center text-neutral-600">—</span>
  }

  return (
    <span
      title={`${buyTypeLabels[economy.buyType]} · $${economy.equipmentValue.toLocaleString('pl-PL')}`}
      className="w-8 rounded bg-neutral-800 px-1 text-center text-xs font-semibold text-neutral-200"
    >
      {buyTypeShort[economy.buyType]}
    </span>
  )
}

function killTags(kill: RoundKill): string {
  const tags = [
    kill.isOpening && 'otwarcie',
    kill.headshot && 'HS',
    kill.wallbang && 'przez ścianę',
    kill.throughSmoke && 'przez smoke',
    kill.noScope && 'noscope',
    kill.attackerBlind && 'oślepiony',
    kill.isTeamKill && 'teamkill',
  ].filter(Boolean)
  return tags.length ? ` (${tags.join(', ')})` : ''
}

/** One round of the timeline: number, result, our side, both buys, end reason, plant site and matched tactic; expands to
 * the kill list with a "replay" button. */
export const RoundRow = memo(function RoundRow({ round, expanded, onToggle, onReplay, tacticMatch }: RoundRowProps) {
  const resultColor = round.weWon === null ? 'bg-neutral-700' : round.weWon ? 'bg-success-500' : 'bg-danger-500'

  return (
    <li className="rounded-md border border-neutral-800">
      <button
        type="button"
        onClick={() => onToggle(round.number)}
        className="flex w-full flex-wrap items-center gap-3 px-3 py-2 text-left text-sm hover:bg-neutral-900"
      >
        <span className="w-8 font-mono text-neutral-400">R{round.number}</span>
        <span className={`h-3 w-3 rounded-full ${resultColor}`} />
        <span className="w-14 font-mono">
          {round.ourScoreAfter}:{round.opponentScoreAfter}
        </span>
        <span className="w-8 text-neutral-400">{round.ourSide ?? '?'}</span>
        <span className="flex items-center gap-1">
          <BuyBadge economy={round.ourEconomy} />
          <span className="text-neutral-600">vs</span>
          <BuyBadge economy={round.opponentEconomy} />
        </span>
        <span className="min-w-28 text-neutral-300">{endReasonLabels[round.endReason]}</span>
        {round.bomb && (
          <span className="text-warning-400">
            Plant {round.bomb.site ?? '?'} {formatRoundTime(round.bomb.plantSecondsIntoRound)}
            {round.bomb.defused ? ' · rozbrojona' : ''}
          </span>
        )}
        {tacticMatch && (
          <span className="text-info-300" title="Automatyczne dopasowanie pozycji i granatów z pierwszych sekund rundy">
            Taktyka: {tacticMatch.tacticName} (dopasowanie {tacticMatch.scorePercent}%)
          </span>
        )}
        <span className="ml-auto text-neutral-500">{expanded ? '▲' : '▼'}</span>
      </button>

      {expanded && (
        <ul className="flex flex-col gap-1 border-t border-neutral-800 px-3 py-2 text-sm">
          <li>
            <button
              type="button"
              onClick={() => onReplay(round.number)}
              className="rounded-md border border-neutral-700 px-3 py-1 text-xs text-neutral-200 transition hover:border-neutral-400"
            >
              ▶ Odtwórz
            </button>
          </li>
          {round.kills.length === 0 && <li className="text-neutral-500">Brak zabójstw w tej rundzie.</li>}
          {round.kills.map((kill, index) => (
            <li key={index} className={kill.byUs === null ? 'text-neutral-400' : kill.byUs ? 'text-success-300' : 'text-danger-300'}>
              <span className="mr-2 font-mono text-neutral-500">{formatRoundTime(kill.secondsIntoRound)}</span>
              {kill.killerName ?? 'świat'} → {kill.victimName}
              <span className="text-neutral-500">
                {' '}
                [{kill.weapon}]{killTags(kill)}
                {kill.victimZone ? ` · ${kill.victimZone}` : ''}
                {kill.assisterName ? ` · asysta: ${kill.assisterName}` : ''}
              </span>
            </li>
          ))}
        </ul>
      )}
    </li>
  )
})
