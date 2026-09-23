import { useMemo, useState } from 'react'
import type { MatchCategory } from '../../../services/resultsApi'
import type { PlayerLeaderboardEntry } from '../../../services/statsApi'
import { matchCategories, matchCategoryLabels } from '../../results/labels'
import { usePlayerLeaderboard } from '../hooks/useStats'

type SortKey = 'matchesPlayed' | 'avgKills' | 'avgDeaths' | 'avgAssists' | 'avgAdr' | 'avgHeadshotPercentage' | 'avgKastPercentage' | 'avgRating'

const columns: { key: SortKey; label: string; title: string }[] = [
  { key: 'matchesPlayed', label: 'Mecze', title: 'Liczba rozegranych meczów w tym filtrze' },
  { key: 'avgKills', label: 'K', title: 'Średnie zabójstwa na mecz' },
  { key: 'avgDeaths', label: 'D', title: 'Średnie śmierci na mecz' },
  { key: 'avgAssists', label: 'A', title: 'Średnie asysty na mecz' },
  { key: 'avgAdr', label: 'ADR', title: 'Średnie obrażenia na rundę' },
  { key: 'avgHeadshotPercentage', label: 'HS%', title: 'Średni procent zabójstw w głowę' },
  { key: 'avgKastPercentage', label: 'KAST%', title: 'Średni KAST% (tylko mecze z importu demki)' },
  { key: 'avgRating', label: 'Rating', title: 'Średni rating' },
]

function sortValue(entry: PlayerLeaderboardEntry, key: SortKey): number {
  return entry[key] ?? -1
}

/** Team-wide leaderboard: every player's stats averaged across their matches, sortable by column and filterable
 * by match category, so teammates can compare performance against each other. */
export function PlayerLeaderboard() {
  const [category, setCategory] = useState<MatchCategory | 'all'>('all')
  const [sortKey, setSortKey] = useState<SortKey>('avgRating')
  const [sortDesc, setSortDesc] = useState(true)
  const { data: entries, isLoading, isError } = usePlayerLeaderboard(category === 'all' ? undefined : category)

  const sorted = useMemo(() => {
    if (!entries) return []
    const copy = [...entries]
    copy.sort((a, b) => (sortValue(a, sortKey) - sortValue(b, sortKey)) * (sortDesc ? -1 : 1))
    return copy
  }, [entries, sortKey, sortDesc])

  function handleSort(key: SortKey) {
    if (key === sortKey) {
      setSortDesc((current) => !current)
    } else {
      setSortKey(key)
      setSortDesc(true)
    }
  }

  return (
    <section className="flex w-full max-w-4xl flex-col gap-3">
      <div className="flex items-center justify-between">
        <h2 className="font-medium">Ranking zawodników</h2>
        <select
          value={category}
          onChange={(event) => setCategory(event.target.value as MatchCategory | 'all')}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          <option value="all">Wszystkie mecze</option>
          {matchCategories.map((value) => (
            <option key={value} value={value}>
              {matchCategoryLabels[value]}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <p className="text-neutral-400">Ładowanie…</p>}
      {isError && <p className="text-red-400">Nie udało się pobrać rankingu.</p>}

      {entries?.length === 0 && (
        <p className="text-neutral-400">Brak statystyk dla tego filtra.</p>
      )}

      {sorted.length > 0 && (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-neutral-500">
              <tr>
                <th className="pb-2 pr-3 font-normal">Gracz</th>
                {columns.map((column) => (
                  <th
                    key={column.key}
                    title={column.title}
                    onClick={() => handleSort(column.key)}
                    className={`cursor-pointer select-none pb-2 pr-3 font-normal hover:text-neutral-300 ${
                      sortKey === column.key ? 'text-neutral-200' : ''
                    }`}
                  >
                    {column.label}
                    {sortKey === column.key ? (sortDesc ? ' ↓' : ' ↑') : ''}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {sorted.map((entry) => (
                <tr key={entry.userId} className="border-t border-neutral-900 text-neutral-300">
                  <td className="py-1.5 pr-3 font-medium">{entry.inGameNickname ?? entry.displayName}</td>
                  <td className="pr-3">{entry.matchesPlayed}</td>
                  <td className="pr-3">{entry.avgKills.toFixed(1)}</td>
                  <td className="pr-3">{entry.avgDeaths.toFixed(1)}</td>
                  <td className="pr-3">{entry.avgAssists.toFixed(1)}</td>
                  <td className="pr-3">{entry.avgAdr.toFixed(1)}</td>
                  <td className="pr-3">{entry.avgHeadshotPercentage.toFixed(0)}%</td>
                  <td className="pr-3">{entry.avgKastPercentage !== null ? `${entry.avgKastPercentage.toFixed(0)}%` : '—'}</td>
                  <td className="pr-3 font-medium text-neutral-100">{entry.avgRating.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
