import { useMyStatsHistory } from '../hooks/useStats'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

/** Table view of the current user's stats across every logged match, oldest first. */
export function MyStatsHistory() {
  const { data: history, isLoading, isError } = useMyStatsHistory()

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie Twoich statystyk…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać statystyk.</p>
  }

  if (history?.length === 0) {
    return <p className="text-neutral-400">Nie masz jeszcze wpisanych statystyk w żadnym meczu.</p>
  }

  const avgRating = history ? history.reduce((sum, entry) => sum + entry.rating, 0) / history.length : 0

  return (
    <div className="w-full max-w-2xl">
      <p className="mb-2 text-sm text-neutral-400">
        Średni rating: <span className="font-medium text-neutral-200">{avgRating.toFixed(2)}</span> z{' '}
        {history?.length} meczów
      </p>

      <div className="overflow-x-auto rounded-md border border-neutral-800">
        <table className="w-full text-left text-sm">
          <thead className="text-neutral-500">
            <tr>
              <th className="p-2 font-normal">Data</th>
              <th className="p-2 font-normal">Przeciwnik</th>
              <th className="p-2 font-normal">K</th>
              <th className="p-2 font-normal">D</th>
              <th className="p-2 font-normal">A</th>
              <th className="p-2 font-normal">ADR</th>
              <th className="p-2 font-normal">HS%</th>
              <th className="p-2 font-normal">Rating</th>
            </tr>
          </thead>
          <tbody>
            {history?.map((entry) => (
              <tr key={entry.matchResultId} className="border-t border-neutral-800 text-neutral-300">
                <td className="p-2">{dateFormatter.format(new Date(entry.playedAtUtc))}</td>
                <td className="p-2">{entry.opponent}</td>
                <td className="p-2">{entry.kills}</td>
                <td className="p-2">{entry.deaths}</td>
                <td className="p-2">{entry.assists}</td>
                <td className="p-2">{entry.adr.toFixed(0)}</td>
                <td className="p-2">{entry.headshotPercentage.toFixed(0)}</td>
                <td className="p-2">{entry.rating.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
