import { useDeleteVacation, useVacations } from '../hooks/useAvailability'
import { vacationIcon } from '../labels'
import { parseIsoDate } from '../weekDates'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

/** Lists the signed-in user's own time-off ranges with a delete action. */
export function VacationList() {
  const { data: vacations, isLoading, isError } = useVacations()
  const deleteVacation = useDeleteVacation()

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie urlopów…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać urlopów.</p>
  }

  if (vacations === undefined || vacations.length === 0) {
    return <p className="text-neutral-400">Brak zaplanowanych urlopów.</p>
  }

  return (
    <ul className="flex w-full max-w-xl flex-col gap-2">
      {vacations.map((vacation) => (
        <li
          key={vacation.id}
          className="flex items-center justify-between gap-3 rounded-md border border-neutral-800 px-3 py-2"
        >
          <div className="min-w-0">
            <p className="truncate text-sm text-neutral-200">
              {vacationIcon} {dateFormatter.format(parseIsoDate(vacation.startDate))} –{' '}
              {dateFormatter.format(parseIsoDate(vacation.endDate))}
            </p>
            {vacation.reason !== null && (
              <p className="truncate text-xs text-neutral-500">{vacation.reason}</p>
            )}
          </div>
          <button
            type="button"
            onClick={() => deleteVacation.mutate(vacation.id)}
            disabled={deleteVacation.isPending}
            className="rounded-md border border-neutral-700 px-3 py-1 text-xs text-neutral-300 transition hover:border-red-500 hover:text-red-400 disabled:opacity-50"
          >
            Usuń
          </button>
        </li>
      ))}
    </ul>
  )
}
