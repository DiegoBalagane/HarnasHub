import { memo } from 'react'
import type { ReplayPlayer, RoundReplay } from '../../../services/replayApi'
import { sideColors } from '../replay/drawReplay'
import { formatRoundTime } from '../labels'

interface ReplayKillFeedProps {
  replay: RoundReplay
  second: number
  onSeek: (second: number) => void
}

function PlayerName({ player }: { player: ReplayPlayer | undefined }) {
  if (!player) return <span className="text-neutral-400">świat</span>
  return <span style={{ color: sideColors[player.side] }}>{player.name}</span>
}

/** Kills of the round: past ones bright, upcoming dimmed; clicking one jumps the replay just before it. */
export const ReplayKillFeed = memo(function ReplayKillFeed({ replay, second, onSeek }: ReplayKillFeedProps) {
  if (replay.kills.length === 0) {
    return <p className="text-sm text-neutral-500">Brak zabójstw w tej rundzie.</p>
  }

  return (
    <ul className="flex max-h-72 flex-col gap-1 overflow-y-auto text-sm">
      {replay.kills.map((kill, index) => (
        <li key={index}>
          <button
            type="button"
            onClick={() => onSeek(Math.max(0, Math.floor(kill.second) - 2))}
            className={`w-full text-left transition hover:text-white ${kill.second < second + 1 ? '' : 'opacity-40'}`}
          >
            <span className="mr-2 font-mono text-neutral-500">{formatRoundTime(kill.second)}</span>
            <PlayerName player={kill.killer === null ? undefined : replay.players[kill.killer]} />
            <span className="text-neutral-500"> → </span>
            <PlayerName player={replay.players[kill.victim]} />
            <span className="text-neutral-500">
              {' '}
              [{kill.weapon}
              {kill.headshot ? ', HS' : ''}
              {kill.isTeamKill ? ', teamkill' : ''}]
            </span>
          </button>
        </li>
      ))}
    </ul>
  )
})
