import { scoreToneClass } from '../../results/scoreTone'
import { Link } from 'react-router-dom'
import type { DashboardSummary as DashboardData } from '../../../services/dashboardApi'
import type { TeamTrendPoint } from '../../../services/statsApi'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { eventTypeLabels } from '../../calendar/labels'
import { opponentProfilePath, opponentReportPath } from '../../opponents/paths'
import { useTeamTrend } from '../../stats/hooks/useStats'
import { useDashboard } from '../hooks/useDashboard'
import { DailyStatusCard } from './DailyStatusCard'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })

function Tile({ to, title, children }: { to: string; title: string; children: React.ReactNode }) {
  return (
    <Link to={to} className="rounded-md border border-neutral-800 p-4 transition hover:border-primary-500/60">
      <p className="text-sm text-neutral-400">{title}</p>
      {children}
    </Link>
  )
}

function NextMatchBanner({ match }: { match: NonNullable<DashboardData['nextMatch']> }) {
  return (
    <div className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-primary-700/60 bg-primary-950/20 p-4">
      <div>
        <p className="text-sm text-primary-300">Najbliższy mecz</p>
        <p className="mt-1 text-lg font-medium">
          vs {match.opponent} · {dateFormatter.format(new Date(match.startsAtUtc))}
        </p>
      </div>
      <div className="flex flex-col items-end gap-1 text-sm text-primary-300">
        <Link to={`/calendar?event=${match.id}`} className="hover:underline">
          Plan meczu i veto →
        </Link>
        <Link to={opponentProfilePath(match.opponent ?? '')} className="hover:underline">
          Przeciwnik: notatki, bilans, mapy →
        </Link>
        <Link to={opponentReportPath(match.opponent ?? '')} className="hover:underline">
          Raport rywala (FACEIT) →
        </Link>
      </div>
    </div>
  )
}

function NextEventTile({ data }: { data: DashboardData | undefined }) {
  return (
    <Tile to={data?.nextEvent ? `/calendar?event=${data.nextEvent.id}` : '/calendar'} title="Najbliższe wydarzenie">
      {data?.nextEvent ? (
        <>
          <p className="mt-1 font-medium">{data.nextEvent.title}</p>
          <p className="text-sm text-neutral-400">
            {eventTypeLabels[data.nextEvent.type]} · {dateFormatter.format(new Date(data.nextEvent.startsAtUtc))}
          </p>
        </>
      ) : (
        <p className="mt-1 text-neutral-500">Brak zaplanowanych wydarzeń</p>
      )}
    </Tile>
  )
}

function TasksTile({ count }: { count: number }) {
  return (
    <Tile to="/tasks" title="Otwarte zadania">
      <p className="mt-1 text-2xl font-semibold">{count}</p>
    </Tile>
  )
}

function TeamFormTile({ latest }: { latest: TeamTrendPoint | null }) {
  return (
    <Tile to="/stats" title="Skuteczność drużyny">
      {latest ? (
        <>
          <p className="mt-1 text-2xl font-semibold">{latest.winRatePercentage.toFixed(0)}%</p>
          <p className="text-sm text-neutral-400">
            {latest.cumulativeWins}W / {latest.cumulativeLosses}L
          </p>
        </>
      ) : (
        <p className="mt-1 text-neutral-500">Brak danych — dodaj wynik w Wynikach</p>
      )}
    </Tile>
  )
}

function MyFormTile({ data }: { data: DashboardData | undefined }) {
  return (
    <Tile to="/stats" title="Moja skuteczność">
      {data?.myRecentPerformance ? (
        <>
          <p className="mt-1 text-2xl font-semibold">{data.myRecentPerformance.avgRating.toFixed(2)}</p>
          <p className="text-sm text-neutral-400">Ostatnie {data.myRecentPerformance.matchesCounted} mecze</p>
        </>
      ) : (
        <p className="mt-1 text-neutral-500">Brak statystyk — pojawią się po imporcie demki</p>
      )}
    </Tile>
  )
}

function LastMatchTile({ data }: { data: DashboardData | undefined }) {
  const last = data?.lastMatch
  return (
    <Tile to={last ? `/results/${last.matchResultId}` : '/results'} title="Ostatni mecz">
      {last ? (
        <>
          <p className="mt-1 font-medium">
            vs {last.opponent}{' '}
            <span className={scoreToneClass(last.ourScore, last.opponentScore)}>
              {last.ourScore}:{last.opponentScore}
            </span>
          </p>
          <p className="text-sm text-neutral-400">
            {last.mapName ? `${last.mapName} · ` : ''}
            {dateFormatter.format(new Date(last.playedAtUtc))}
          </p>
        </>
      ) : (
        <p className="mt-1 text-neutral-500">Brak wyników</p>
      )}
    </Tile>
  )
}

function AttendanceTile({ data }: { data: DashboardData | undefined }) {
  return (
    <Tile to="/attendance" title="Spóźnienia i nieobecności">
      <p className="mt-1 text-2xl font-semibold">
        {data?.attendance.lateCount ?? 0} / {data?.attendance.absentCount ?? 0}
      </p>
      <p className="text-sm text-neutral-400">Ostatnie 30 dni</p>
    </Tile>
  )
}

/** Dashboard body. Coach/Manager see the next match with its links first, then team-wide tiles; players see availability, own tasks and form first. */
export function DashboardSummary() {
  const { data, isLoading, isError } = useDashboard()
  const { data: trend } = useTeamTrend()
  const isCoach = useIsCoachOrManager()
  const latestTrend = trend && trend.length > 0 ? trend[trend.length - 1] : null

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie…</p>
  }

  if (isError) {
    return <p className="text-danger-400">Nie udało się pobrać podsumowania.</p>
  }

  const availability = data && (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <DailyStatusCard title="Dzisiaj" day={data.today} />
      <DailyStatusCard title="Jutro" day={data.tomorrow} />
    </div>
  )
  const nextMatch = data?.nextMatch?.opponent ? <NextMatchBanner match={data.nextMatch} /> : null
  const tileGrid = 'grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3'

  if (isCoach) {
    return (
      <div className="flex w-full flex-col gap-4">
        {nextMatch}
        <div className={tileGrid}>
          <NextEventTile data={data} />
          <LastMatchTile data={data} />
          <AttendanceTile data={data} />
          <TeamFormTile latest={latestTrend} />
          <TasksTile count={data?.openTaskCount ?? 0} />
          <MyFormTile data={data} />
        </div>
        {availability}
      </div>
    )
  }

  return (
    <div className="flex w-full flex-col gap-4">
      {availability}
      <div className={tileGrid}>
        <TasksTile count={data?.openTaskCount ?? 0} />
        <MyFormTile data={data} />
        <NextEventTile data={data} />
      </div>
      {nextMatch}
      <div className={tileGrid}>
        <LastMatchTile data={data} />
        <TeamFormTile latest={latestTrend} />
        <AttendanceTile data={data} />
      </div>
    </div>
  )
}
