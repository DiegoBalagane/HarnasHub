import { memo } from 'react'
import type { OpponentDemo } from '../../../services/opponentDemosApi'
import { useDeleteOpponentDemo, useSetOpponentDemoTeam } from '../hooks/useOpponentDemos'
import { OpponentDemoReplayPicker } from './OpponentDemoReplayPicker'

interface OpponentDemoListProps {
  opponentName: string
  demos: OpponentDemo[]
  canManage: boolean
}

/** The opponent's analysed demos; an unresolved one asks the coach which team is the opponent. */
export const OpponentDemoList = memo(function OpponentDemoList({
  opponentName,
  demos,
  canManage,
}: OpponentDemoListProps) {
  const setTeam = useSetOpponentDemoTeam(opponentName)
  const remove = useDeleteOpponentDemo(opponentName)

  if (demos.length === 0) {
    return <p className="text-sm text-neutral-400">Brak przeanalizowanych demek rywala.</p>
  }

  return (
    <ul className="flex flex-col gap-2">
      {demos.map((demo) => (
        <li key={demo.id} className="flex flex-col gap-2 rounded-md bg-neutral-900 px-3 py-2 text-sm">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span>
              <span className="font-medium">{demo.mapName ?? demo.rawMapName ?? 'nieznana mapa'}</span>
              <span className="text-neutral-400">
                {' '}
                · {demo.roundsCount} rund · {demo.source === 'FaceitDownload' ? 'FACEIT' : 'wgrana'} ·{' '}
                {new Date(demo.playedAtUtc ?? demo.createdAtUtc).toLocaleDateString('pl-PL')}
              </span>
            </span>
            <span className="flex items-center gap-3">
              <OpponentDemoReplayPicker demo={demo} opponentName={opponentName} />
              {demo.teamResolved ? (
                <span className="text-xs text-success-400">rywal: drużyna {demo.opponentTeam}</span>
              ) : (
                <span className="text-xs text-warning-300">nie rozpoznano rywala</span>
              )}
              {canManage && (
                <button
                  type="button"
                  disabled={remove.isPending}
                  onClick={() => remove.mutate(demo.id)}
                  className="text-xs text-danger-400 hover:underline disabled:opacity-50"
                >
                  Usuń
                </button>
              )}
            </span>
          </div>

          {canManage && !demo.teamResolved && (
            <div className="grid gap-2 sm:grid-cols-2">
              {demo.teams.map((team) => (
                <button
                  key={team.team}
                  type="button"
                  disabled={setTeam.isPending}
                  onClick={() => setTeam.mutate({ demoId: demo.id, team: team.team })}
                  className={`rounded-md border px-2 py-1 text-left text-xs transition hover:border-primary-500 ${
                    demo.opponentTeam === team.team ? 'border-primary-500' : 'border-neutral-800'
                  }`}
                >
                  <span className="block font-medium">
                    To rywal: drużyna {team.team} ({team.team === 'A' ? 'start T' : 'start CT'})
                  </span>
                  <span className="text-neutral-400">{team.names.join(', ')}</span>
                </button>
              ))}
            </div>
          )}
        </li>
      ))}
      {(setTeam.isError || remove.isError) && (
        <li className="text-sm text-danger-400">{(setTeam.error ?? remove.error)?.message}</li>
      )}
    </ul>
  )
})
