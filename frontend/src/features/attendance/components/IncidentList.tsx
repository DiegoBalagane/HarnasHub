import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { parseIsoDate } from '../../availability/weekDates'
import { useAttendanceIncidents, useDeleteIncident } from '../hooks/useAttendance'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

const typeLabels: Record<string, string> = {
  Late: 'Spóźnienie',
  Absent: 'Nieobecność',
}

/** Flat feed of every logged incident, newest first — visible to everyone, removable by Coach/Manager. */
export function IncidentList() {
  const { data: incidents, isLoading, isError } = useAttendanceIncidents()
  const deleteIncident = useDeleteIncident()
  const canManage = useIsCoachOrManager()

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać wpisów.</p>
  }

  if (incidents?.length === 0) {
    return <p className="text-neutral-400">Brak wpisów.</p>
  }

  return (
    <ul className="flex flex-col gap-2">
      {incidents?.map((incident) => (
        <li
          key={incident.id}
          className="flex items-center justify-between gap-3 rounded-md border border-neutral-800 px-3 py-2 text-sm"
        >
          <div>
            <span className="font-medium">{incident.playerName}</span>{' '}
            <span className={incident.type === 'Absent' ? 'text-red-400' : 'text-amber-400'}>
              {typeLabels[incident.type]}
            </span>{' '}
            <span className="text-neutral-500">· {dateFormatter.format(parseIsoDate(incident.occurredOn))}</span>
            {incident.note && <p className="text-xs text-neutral-500">{incident.note}</p>}
          </div>

          {canManage && (
            <button
              type="button"
              onClick={() => deleteIncident.mutate(incident.id)}
              disabled={deleteIncident.isPending}
              className="text-xs text-neutral-500 transition hover:text-red-400 disabled:opacity-50"
            >
              Usuń
            </button>
          )}
        </li>
      ))}
    </ul>
  )
}
