import { useState } from 'react'
import { DatePicker } from '../../../components/ui/DateTimePicker'
import { useDeleteVacation, useUpdateVacation, useVacations } from '../hooks/useAvailability'
import { vacationIcon } from '../labels'
import { parseIsoDate } from '../weekDates'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })
const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

/** Lists the signed-in user's own upcoming/ongoing time-off ranges (past ones are hidden — nothing to do with
 * them), with inline edit and delete actions. */
export function VacationList() {
  const { data: vacations, isLoading, isError } = useVacations()
  const deleteVacation = useDeleteVacation()
  const [editingId, setEditingId] = useState<string | null>(null)

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie urlopów…</p>
  }

  if (isError) {
    return <p className="text-danger-400">Nie udało się pobrać urlopów.</p>
  }

  const today = new Date().toISOString().slice(0, 10)
  const upcoming = (vacations ?? []).filter((vacation) => vacation.endDate >= today)

  if (upcoming.length === 0) {
    return <p className="text-neutral-400">Brak zaplanowanych urlopów.</p>
  }

  return (
    <ul className="flex w-full flex-col gap-2">
      {upcoming.map((vacation) =>
        editingId === vacation.id ? (
          <VacationEditRow key={vacation.id} vacation={vacation} onDone={() => setEditingId(null)} />
        ) : (
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
            <div className="flex shrink-0 gap-2">
              <button
                type="button"
                onClick={() => setEditingId(vacation.id)}
                className="rounded-md border border-neutral-700 px-3 py-1 text-xs text-neutral-300 transition hover:border-neutral-500"
              >
                Edytuj
              </button>
              <button
                type="button"
                onClick={() => deleteVacation.mutate(vacation.id)}
                disabled={deleteVacation.isPending}
                className="rounded-md border border-neutral-700 px-3 py-1 text-xs text-neutral-300 transition hover:border-danger-500 hover:text-danger-400 disabled:opacity-50"
              >
                Usuń
              </button>
            </div>
          </li>
        ),
      )}
    </ul>
  )
}

interface VacationEditRowProps {
  vacation: { id: string; startDate: string; endDate: string; reason: string | null }
  onDone: () => void
}

/** Inline form replacing one vacation row while it's being edited. */
function VacationEditRow({ vacation, onDone }: VacationEditRowProps) {
  const [startDate, setStartDate] = useState(vacation.startDate)
  const [endDate, setEndDate] = useState(vacation.endDate)
  const [reason, setReason] = useState(vacation.reason ?? '')
  const [validationError, setValidationError] = useState<string | null>(null)
  const updateVacation = useUpdateVacation()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()

    if (endDate < startDate) {
      setValidationError('Data końca nie może być wcześniejsza niż data początku.')
      return
    }

    setValidationError(null)
    updateVacation.mutate(
      {
        vacationId: vacation.id,
        payload: { startDate, endDate, reason: reason.trim() === '' ? null : reason.trim() },
      },
      { onSuccess: onDone },
    )
  }

  return (
    <li className="flex flex-col gap-2 rounded-md border border-neutral-700 bg-neutral-950 px-3 py-2">
      <form onSubmit={handleSubmit} className="flex flex-col gap-2">
        <div className="flex flex-wrap gap-2">
          <DatePicker label="Początek urlopu" value={startDate} onChange={setStartDate} className="flex-1" />
          <DatePicker label="Koniec urlopu" value={endDate} onChange={setEndDate} className="flex-1" />
        </div>

        <input
          maxLength={200}
          placeholder="Powód (opcjonalnie)"
          value={reason}
          onChange={(event) => setReason(event.target.value)}
          className={inputClass}
        />

        {validationError !== null && <p className="text-sm text-danger-400">{validationError}</p>}
        {updateVacation.isError && <p className="text-sm text-danger-400">Nie udało się zapisać zmian.</p>}

        <div className="flex gap-2">
          <button
            type="submit"
            disabled={updateVacation.isPending}
            className="rounded-md bg-primary-500 px-3 py-1.5 text-xs font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
          >
            {updateVacation.isPending ? 'Zapisywanie…' : 'Zapisz'}
          </button>
          <button
            type="button"
            onClick={onDone}
            className="rounded-md border border-neutral-700 px-3 py-1.5 text-xs text-neutral-300 transition hover:border-neutral-500"
          >
            Anuluj
          </button>
        </div>
      </form>
    </li>
  )
}
