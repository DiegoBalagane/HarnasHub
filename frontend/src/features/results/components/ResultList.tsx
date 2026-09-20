import { useMemo, useState } from 'react'
import type { MatchResult } from '../../../services/resultsApi'
import { MatchStatsPanel } from '../../stats/components/MatchStatsPanel'
import { useResults } from '../hooks/useResults'
import { leagueTypeLabels, matchCategoryLabels } from '../labels'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

interface Group {
  key: string
  label: string
  results: MatchResult[]
}

/** Splits results into Scrimmage (flat) / Tournament (per tournament) / League (per season+type) groups, most recent group first. */
function groupResults(results: MatchResult[]): Group[] {
  const groups = new Map<string, Group>()

  for (const result of results) {
    const key =
      result.category === 'Tournament'
        ? `tournament:${result.tournamentId}`
        : result.category === 'League'
          ? `league:${result.leagueId}`
          : 'scrimmage'

    const label =
      result.category === 'Tournament'
        ? (result.tournamentName ?? 'Turniej')
        : result.category === 'League'
          ? `${result.leagueName} — ${result.leagueSeason} (${result.leagueType ? leagueTypeLabels[result.leagueType] : ''})`
          : matchCategoryLabels.Scrimmage

    const existing = groups.get(key)
    if (existing) {
      existing.results.push(result)
    } else {
      groups.set(key, { key, label, results: [result] })
    }
  }

  // Each bucket is already sorted by playedAtUtc (results arrive newest-first); order the buckets
  // themselves by their most recent result so the freshest tournament/league surfaces first.
  return [...groups.values()].sort(
    (a, b) => new Date(b.results[0].playedAtUtc).getTime() - new Date(a.results[0].playedAtUtc).getTime(),
  )
}

/** Lists logged results grouped by tournament/league (scrims stay flat); clicking one expands its per-player stats. */
export function ResultList() {
  const { data: results, isLoading, isError } = useResults()
  const [expandedId, setExpandedId] = useState<string | null>(null)
  const groups = useMemo(() => groupResults(results ?? []), [results])

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
    <div className="flex w-full max-w-xl flex-col gap-5">
      {groups.map((group) => (
        <section key={group.key} className="flex flex-col gap-2">
          <h3 className="text-sm font-semibold text-neutral-300">{group.label}</h3>
          <ul className="flex flex-col gap-3">
            {group.results.map((result) => (
              <ResultCard
                key={result.id}
                result={result}
                isExpanded={expandedId === result.id}
                onToggle={() => setExpandedId(expandedId === result.id ? null : result.id)}
              />
            ))}
          </ul>
        </section>
      ))}
    </div>
  )
}

interface ResultCardProps {
  result: MatchResult
  isExpanded: boolean
  onToggle: () => void
}

function ResultCard({ result, isExpanded, onToggle }: ResultCardProps) {
  const won = result.ourScore > result.opponentScore

  return (
    <li className="rounded-md border border-neutral-800 p-4">
      <button className="flex w-full items-center justify-between text-left" onClick={onToggle}>
        <p className="font-medium">
          vs {result.opponent}{' '}
          <span className={won ? 'text-green-400' : 'text-red-400'}>
            {result.ourScore}:{result.opponentScore}
          </span>
        </p>
        <span className="text-sm text-neutral-500">{dateFormatter.format(new Date(result.playedAtUtc))}</span>
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

      {isExpanded && (
        <div className="mt-3">
          <MatchStatsPanel matchResultId={result.id} />
        </div>
      )}
    </li>
  )
}
