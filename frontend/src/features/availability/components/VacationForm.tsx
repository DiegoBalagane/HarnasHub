import { useState } from 'react'
import { DatePicker } from '../../../components/ui/DateTimePicker'
import { useSetVacation } from '../hooks/useAvailability'

const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

interface VacationFormProps {
  /** Called after a successful submit — e.g. to close the modal hosting this form. */
  onDone?: () => void
}

/** Form for adding a time-off range for the signed-in user. */
export function VacationForm({ onDone }: VacationFormProps) {
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [reason, setReason] = useState('')
  const [validationError, setValidationError] = useState<string | null>(null)
  const setVacation = useSetVacation()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()

    if (endDate < startDate) {
      setValidationError('Data końca nie może być wcześniejsza niż data początku.')
      return
    }

    setValidationError(null)
    setVacation.mutate(
      { startDate, endDate, reason: reason.trim() === '' ? null : reason.trim() },
      {
        onSuccess: () => {
          setStartDate('')
          setEndDate('')
          setReason('')
          onDone?.()
        },
      },
    )
  }

  return (
    <form onSubmit={handleSubmit} className="flex w-full flex-col gap-3">
      <div className="flex flex-wrap gap-3">
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
      {setVacation.isError && <p className="text-sm text-danger-400">Nie udało się dodać urlopu.</p>}

      <button
        type="submit"
        disabled={setVacation.isPending}
        className="self-start rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
      >
        {setVacation.isPending ? 'Dodawanie…' : 'Dodaj urlop'}
      </button>
    </form>
  )
}
