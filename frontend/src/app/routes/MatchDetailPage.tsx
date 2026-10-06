import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { MatchTabs } from '../../features/match-analysis/components/MatchTabs'
import { opponentProfilePath } from '../../features/opponents/paths'
import { EditResultForm } from '../../features/results/components/EditResultForm'
import { useResults } from '../../features/results/hooks/useResults'
import { matchCategoryLabels } from '../../features/results/labels'
import { MatchStatsPanel } from '../../features/stats/components/MatchStatsPanel'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'long', timeStyle: 'short' })

/** Single match page: score, opponent link, map, then tabs — overview (notes + demo insights), round timeline and per-player stats with the death map. Reads the result from the shared results query. */
export function MatchDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: results, isLoading, isError } = useResults()
  const canManage = useIsCoachOrManager()
  const [isEditing, setIsEditing] = useState(false)
  const result = results?.find((candidate) => candidate.id === id)

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie meczu…</p>
  }

  if (isError) {
    return <p className="text-danger-400">Nie udało się pobrać meczu.</p>
  }

  if (!result) {
    return (
      <div className="flex flex-col gap-3">
        <p className="text-neutral-400">Nie znaleziono takiego meczu.</p>
        <Link to="/results" className="text-sm text-primary-400 hover:underline">
          ← Wróć do wyników
        </Link>
      </div>
    )
  }

  const won = result.ourScore > result.opponentScore

  return (
    <div className="flex w-full flex-col gap-5">
      <Link to="/results" className="self-start text-sm text-neutral-500 hover:text-white">
        ← Wyniki
      </Link>

      <header className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold">
            vs {result.opponent}{' '}
            <span className={won ? 'text-success-400' : 'text-danger-400'}>
              {result.ourScore}:{result.opponentScore}
            </span>
          </h1>
          <p className="mt-1 text-sm text-neutral-400">
            {dateFormatter.format(new Date(result.playedAtUtc))} · {matchCategoryLabels[result.category]}
            {result.mapName ? ` · ${result.mapName}` : ''}
            {result.tournamentName ? ` · ${result.tournamentName}` : ''}
            {result.leagueName ? ` · ${result.leagueName}` : ''}
          </p>
        </div>

        <div className="flex items-center gap-2 text-sm">
          <Link
            to={opponentProfilePath(result.opponent)}
            className="rounded-md border border-neutral-800 px-3 py-1.5 text-primary-400 transition hover:border-neutral-600"
          >
            Profil przeciwnika →
          </Link>
          {canManage && (
            <button
              type="button"
              onClick={() => setIsEditing((current) => !current)}
              className="rounded-md border border-neutral-800 px-3 py-1.5 text-neutral-300 transition hover:border-neutral-600"
            >
              {isEditing ? 'Zamknij edycję' : 'Edytuj'}
            </button>
          )}
        </div>
      </header>

      {isEditing && <EditResultForm result={result} onClose={() => setIsEditing(false)} />}

      <MatchTabs
        matchResultId={result.id}
        overview={
          result.notes && <p className="rounded-md border border-neutral-800 p-3 text-sm text-neutral-300">{result.notes}</p>
        }
        players={<MatchStatsPanel matchResultId={result.id} mapName={result.mapName} />}
      />
    </div>
  )
}
