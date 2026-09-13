import { Link } from 'react-router-dom'
import { eventTypeLabels } from '../../calendar/labels'
import { useTeamTrend } from '../../stats/hooks/useStats'
import { useDashboard } from '../hooks/useDashboard'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })

/** Three-tile summary: the next event, open tasks, and the team's current win-rate trend. */
export function DashboardSummary() {
  const { data, isLoading, isError } = useDashboard()
  const { data: trend } = useTeamTrend()
  const latestTrend = trend && trend.length > 0 ? trend[trend.length - 1] : null

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać podsumowania.</p>
  }

  return (
    <div className="grid w-full max-w-2xl grid-cols-1 gap-4 sm:grid-cols-3">
      <Link
        to="/calendar"
        className="rounded-md border border-neutral-800 p-4 transition hover:border-neutral-600"
      >
        <p className="text-sm text-neutral-400">Najbliższe wydarzenie</p>
        {data?.nextEvent ? (
          <>
            <p className="mt-1 font-medium">{data.nextEvent.title}</p>
            <p className="text-sm text-neutral-400">
              {eventTypeLabels[data.nextEvent.type]} ·{' '}
              {dateFormatter.format(new Date(data.nextEvent.startsAtUtc))}
            </p>
          </>
        ) : (
          <p className="mt-1 text-neutral-500">Brak zaplanowanych wydarzeń</p>
        )}
      </Link>

      <Link
        to="/tasks"
        className="rounded-md border border-neutral-800 p-4 transition hover:border-neutral-600"
      >
        <p className="text-sm text-neutral-400">Otwarte zadania</p>
        <p className="mt-1 text-2xl font-semibold">{data?.openTaskCount ?? 0}</p>
      </Link>

      <Link
        to="/stats"
        className="rounded-md border border-neutral-800 p-4 transition hover:border-neutral-600"
      >
        <p className="text-sm text-neutral-400">Skuteczność</p>
        {latestTrend ? (
          <>
            <p className="mt-1 text-2xl font-semibold">{latestTrend.winRatePercentage.toFixed(0)}%</p>
            <p className="text-sm text-neutral-400">
              {latestTrend.cumulativeWins}W / {latestTrend.cumulativeLosses}L
            </p>
          </>
        ) : (
          <p className="mt-1 text-neutral-500">Brak danych</p>
        )}
      </Link>
    </div>
  )
}
