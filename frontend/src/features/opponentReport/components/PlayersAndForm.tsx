import { memo } from 'react'
import type { MapPlayersToWatch, OpponentForm } from '../../../services/opponentReportApi'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'short' })

/** Top opponent players per map from their FACEIT games (team and solo), ranked by ADR. */
export const PlayersToWatch = memo(function PlayersToWatch({ maps }: { maps: MapPlayersToWatch[] }) {
  if (maps.length === 0) {
    return null
  }

  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-medium">Gracze do pilnowania</h2>
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {maps.map((map) => (
          <div key={map.mapName} className="rounded-md border border-neutral-800 p-3">
            <p className="mb-1 text-sm font-medium">{map.mapName}</p>
            <ul className="flex flex-col gap-1 text-sm">
              {map.players.map((player) => (
                <li key={player.playerId} className="flex justify-between gap-2">
                  <span>{player.nickname}</span>
                  <span className="tabular-nums text-xs text-neutral-400">
                    {player.adr !== null && `ADR ${Math.round(player.adr)} · `}K/D {player.kdRatio.toFixed(2)} · {player.games} m.
                  </span>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
    </section>
  )
})

/** Last team games of the opponent with the current streak and new faces in the lineup. */
export const TeamFormSection = memo(function TeamFormSection({ form }: { form: OpponentForm }) {
  if (form.lastGames.length === 0) {
    return null
  }

  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-medium">
        Forma {form.streak && <span className="text-sm text-neutral-400">· seria {form.streak}</span>}
      </h2>
      {form.newPlayers.length > 0 && (
        <p className="text-sm text-warning-300">Nowi w składzie: {form.newPlayers.join(', ')}</p>
      )}
      <ul className="flex flex-col gap-1 text-sm">
        {form.lastGames.map((game) => (
          <li
            key={`${game.faceitMatchId}-${game.mapName}`}
            className="flex justify-between gap-2 rounded-md border border-neutral-800 px-3 py-1.5"
          >
            <span className={game.won ? 'text-success-400' : 'text-danger-400'}>{game.won ? 'W' : 'L'}</span>
            <span className="w-24">{game.mapName ?? '—'}</span>
            <span className="tabular-nums">
              {game.roundsFor}:{game.roundsAgainst}
            </span>
            <span className="flex-1 truncate text-right text-xs text-neutral-500">
              {game.competitionName} · {dateFormatter.format(new Date(game.playedAtUtc))}
            </span>
          </li>
        ))}
      </ul>
    </section>
  )
})
