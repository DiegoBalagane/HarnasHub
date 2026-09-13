import { useResults } from '../hooks/useResults'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

/** Lists logged results with the win/loss outcome highlighted. */
export function ResultList() {
  const { data: results, isLoading, isError } = useResults()

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
            <div className="flex items-center justify-between">
              <p className="font-medium">
                vs {result.opponent}{' '}
                <span className={won ? 'text-green-400' : 'text-red-400'}>
                  {result.ourScore}:{result.opponentScore}
                </span>
              </p>
              <span className="text-sm text-neutral-500">
                {dateFormatter.format(new Date(result.playedAtUtc))}
              </span>
            </div>
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
          </li>
        )
      })}
    </ul>
  )
}
