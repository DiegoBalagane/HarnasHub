import { Link } from 'react-router-dom'
import type { OpponentReport, VetoFormat } from '../../../services/opponentReportApi'
import { JobProgress } from '../../jobs/components/JobProgress'
import { useCreateMatchPlan, useRefreshOpponentReport } from '../hooks/useOpponentReport'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })

/** Props of {@link ReportToolbar}. */
interface ReportToolbarProps {
  report: OpponentReport
  canManage: boolean
  format: VetoFormat
}

/** Data freshness, "Odśwież dane" and "Utwórz plan meczu" (only when a match against them is scheduled). */
export function ReportToolbar({ report, canManage, format }: ReportToolbarProps) {
  const refresh = useRefreshOpponentReport(report.opponentName)
  const createPlan = useCreateMatchPlan()
  const syncedAt = report.dataSyncedAtUtc

  return (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-800 p-3 text-sm">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-neutral-400">
          {syncedAt ? `Dane z FACEIT: ${dateFormatter.format(new Date(syncedAt))}` : 'Dane z FACEIT nie były jeszcze pobrane'}
          {' · '}
          {report.theirTeamGames} meczów drużynowych rywala, {report.theirSoloGames} solo · nas na FACEIT: {report.ourLinkedPlayers}
        </p>
        {canManage && (
          <div className="flex gap-2">
            {report.link && report.faceitConfigured && (
              <button
                type="button"
                onClick={() => refresh.start()}
                disabled={refresh.isBusy}
                className="rounded-md border border-neutral-700 px-3 py-1 hover:border-neutral-500 disabled:opacity-50"
              >
                {refresh.isBusy ? 'Pobieranie…' : 'Odśwież dane'}
              </button>
            )}
            {report.nextEventId && (
              <button
                type="button"
                onClick={() => createPlan.mutate({ report, format })}
                disabled={createPlan.isPending}
                className="rounded-md bg-primary-500 px-3 py-1 font-medium text-primary-950 hover:bg-primary-400 disabled:opacity-50"
                title={`Dopisze TL;DR i veto ${format.toUpperCase()} do planu meczu z ${dateFormatter.format(new Date(report.nextEventAtUtc ?? ''))}`}
              >
                Utwórz plan meczu
              </button>
            )}
          </div>
        )}
      </div>
      {!report.faceitConfigured && (
        <p className="text-warning-300">Integracja z FACEIT nie jest skonfigurowana na serwerze — raport pokazuje tylko wcześniej zapisane dane.</p>
      )}
      <JobProgress job={refresh.job} isStarting={refresh.isStarting} error={refresh.error} />
      {createPlan.isError && <p className="text-danger-400">{createPlan.error.message}</p>}
      {createPlan.isSuccess && (
        <p className="text-success-400">
          Zapisano w planie meczu — <Link to={`/calendar?event=${createPlan.data.eventId}`} className="underline">otwórz wydarzenie</Link>.
        </p>
      )}
    </div>
  )
}
