import { memo } from 'react'
import { faceitMatchRoomUrl } from '../../../constants'
import type { FormGame } from '../../../services/opponentReportApi'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'short' })

interface FaceitMatchLinksProps {
  games: FormGame[]
  /** FACEIT match ids that already have an analysed demo in this report. */
  analysedMatchIds: ReadonlySet<string>
}

/** Their recent FACEIT team matches with a link to each match room, so a coach can download the demo by hand and
 * upload it — the manual path while automatic FACEIT downloads aren't available. Already analysed matches are marked. */
export const FaceitMatchLinks = memo(function FaceitMatchLinks({ games, analysedMatchIds }: FaceitMatchLinksProps) {
  if (games.length === 0) {
    return null
  }

  const missing = games.filter((game) => !analysedMatchIds.has(game.faceitMatchId)).length

  return (
    <div className="flex flex-col gap-2 rounded-md border border-surface-border bg-surface-card p-4">
      <div>
        <h3 className="text-sm font-medium">Ich ostatnie mecze na FACEIT</h3>
        <p className="mt-1 text-xs text-neutral-400">
          Otwórz pokój meczu, pobierz demkę i wgraj ją powyżej — mecz i drużyna rywala rozpoznają się same z nazwy pliku.
          {missing > 0 && ` Do przeanalizowania: ${missing} z ${games.length}.`}
        </p>
      </div>
      <ul className="flex flex-col divide-y divide-surface-border">
        {games.map((game) => {
          const analysed = analysedMatchIds.has(game.faceitMatchId)
          return (
            <li key={game.faceitMatchId} className="flex flex-wrap items-center gap-x-3 gap-y-1 py-2 text-sm">
              <span className={`w-5 font-semibold ${game.won ? 'text-success-400' : 'text-danger-400'}`}>
                {game.won ? 'W' : 'L'}
              </span>
              <span className="w-20 text-neutral-300">{game.mapName ?? '—'}</span>
              <span className="w-14 tabular-nums">
                {game.roundsFor}:{game.roundsAgainst}
              </span>
              <span className="text-neutral-500">{dateFormatter.format(new Date(game.playedAtUtc))}</span>
              {game.competitionName && <span className="truncate text-xs text-neutral-500">{game.competitionName}</span>}
              <span className="ml-auto">
                {analysed ? (
                  <span className="rounded-full border border-success-700 px-2 py-0.5 text-xs text-success-300">
                    Przeanalizowana
                  </span>
                ) : (
                  <a
                    href={faceitMatchRoomUrl(game.faceitMatchId)}
                    target="_blank"
                    rel="noreferrer"
                    className="text-xs text-primary-400 hover:underline"
                  >
                    Pokój meczu ↗
                  </a>
                )}
              </span>
            </li>
          )
        })}
      </ul>
    </div>
  )
})
