import { useState } from 'react'
import { MatchStatsPanel } from '../../stats/components/MatchStatsPanel'
import { useResults } from '../hooks/useResults'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

/** Lists logged results with the win/loss outcome highlighted; clicking one expands its per-player stats. */
export function ResultList() {
  const { data: results, isLoading, isError } = useResults()
  const [expandedId, setExpandedId] = useState<string | null>(null)

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie wyników…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać wyników.</p>
  }

  if (results?.length === 0) {
    return <p className="text-neutral-400">Brak zapisanych wyników.</p>
  }

  return (
    <ul className="flex w-full max-w-xl flex-col gap-3">
      {results?.map((result) => {
        const won = result.ourScore > result.opponentScore
        return (
          <li key={result.id} className="rounded-md border border-neutral-800 p-4">
            <button
              className="flex w-full items-center justify-between text-left"
              onClick={() => setExpandedId(expandedId === result.id ? null : result.id)}
            >
              <p className="font-medium">
                vs {result.opponent}{' '}
                <span className={won ? 'text-green-400' : 'text-red-400'}>
                  {result.ourScore}:{result.opponentScore}
                </span>
              </p>
              <span className="text-sm text-neutral-500">
                {dateFormatter.format(new Date(result.playedAtUtc))}
              </span>
            </button>
            {result.mapName && <p className="text-sm text-neutral-400">Mapa: {result.mapName}</p>}
            {result.notes && <p className="mt-1 text-sm text-neutral-400">{result.notes}</p>}
            {result.demoUrl && (
              <a
                href={result.demoUrl}
                target="_blank"
                rel="noreferrer"
                className="mt-1 inline-block text-sm text-red-400 hover:underline"
              >
                Demka
              </a>
            )}

            {expandedId === result.id && (
              <div className="mt-3">
                <MatchStatsPanel matchResultId={result.id} />
              </div>
            )}
          </li>
        )
      })}
    </ul>
  )
}
