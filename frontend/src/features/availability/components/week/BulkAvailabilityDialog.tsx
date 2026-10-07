import { useState } from 'react'
import { TimeRangePicker } from '../../../../components/ui/DateTimePicker'
import { Modal } from '../../../../components/Modal'
import type { MemberWeek } from '../../../../services/availabilityApi'
import { chipSelection, type BulkChip } from '../../bulkDays'
import { useSetDayAvailability } from '../../hooks/useAvailability'
import { entryFor, parseIsoDate, weekdayLabelFor } from '../../weekDates'

const dayFormatter = new Intl.DateTimeFormat('pl-PL', { day: '2-digit', month: '2-digit' })
const chips: { id: BulkChip; label: string }[] = [
  { id: 'all', label: 'Wszystkie' },
  { id: 'workdays', label: 'Dni robocze' },
  { id: 'weekend', label: 'Weekend' },
]
const chipClass =
  'rounded-full border border-neutral-700 px-2.5 py-0.5 text-xs text-neutral-300 transition hover:border-neutral-500'

interface BulkAvailabilityDialogProps {
  member: MemberWeek
  /** All days of the shown week. */
  dates: string[]
  /** Subset that may be edited (not past, not on vacation) — also the default selection. */
  eligible: string[]
  onClose: () => void
}

/** Sets one availability (status + optional hours) for many days of the shown week with one click; saves each selected day through the regular per-day API and reports failures per day. */
export function BulkAvailabilityDialog({ member, dates, eligible, onClose }: BulkAvailabilityDialogProps) {
  const [selected, setSelected] = useState<string[]>(eligible)
  const [status, setStatus] = useState<'Available' | 'Off'>('Available')
  const [fullDay, setFullDay] = useState(true)
  const [from, setFrom] = useState('18:00')
  const [to, setTo] = useState('22:00')
  const [error, setError] = useState<string | null>(null)
  const [failedDays, setFailedDays] = useState<string[]>([])
  const [isSaving, setIsSaving] = useState(false)
  const { mutateAsync } = useSetDayAvailability()
  const isPartial = status === 'Available' && !fullDay

  function toggle(date: string) {
    setSelected((current) => (current.includes(date) ? current.filter((value) => value !== date) : [...current, date]))
  }

  async function apply() {
    if (selected.length === 0) {
      setError('Zaznacz co najmniej jeden dzień.')
      return
    }
    if (isPartial && from >= to) {
      setError('Godzina od musi być wcześniejsza niż godzina do.')
      return
    }

    setError(null)
    setIsSaving(true)
    const days = dates.filter((date) => selected.includes(date))
    const results = await Promise.allSettled(
      days.map((date) =>
        mutateAsync({
          date,
          status: status === 'Off' ? 'Off' : isPartial ? 'PartiallyAvailable' : 'Available',
          availableFromLocal: isPartial ? from : null,
          availableToLocal: isPartial ? to : null,
          // Keep each day's existing note — a bulk change is about status, not about wiping notes.
          note: entryFor(member, date)?.note ?? null,
        }),
      ),
    )
    setIsSaving(false)

    const failed = days.filter((_, index) => results[index].status === 'rejected')
    if (failed.length === 0) onClose()
    else {
      setFailedDays(failed)
      setSelected(failed)
      setError('Nie udało się zapisać części dni — zaznaczono tylko te, które trzeba ponowić.')
    }
  }

  return (
    <Modal title="Ustaw dla tygodnia" onClose={onClose}>
      <div className="flex flex-col gap-3">
        <div className="flex flex-wrap gap-1.5" aria-label="Szybki wybór dni">
          {chips.map((chip) => (
            <button key={chip.id} type="button" className={chipClass} onClick={() => setSelected(chipSelection(chip.id, eligible))}>
              {chip.label}
            </button>
          ))}
        </div>

        <ul className="grid grid-cols-2 gap-1 sm:grid-cols-4">
          {dates.map((date) => {
            const isEligible = eligible.includes(date)
            const reason = entryFor(member, date)?.isVacation ? 'urlop' : 'minął'

            return (
              <li key={date}>
                <label className="flex items-center gap-1.5 text-xs text-neutral-300 has-[:disabled]:text-neutral-600">
                  <input
                    type="checkbox"
                    disabled={!isEligible}
                    checked={selected.includes(date)}
                    onChange={() => toggle(date)}
                  />
                  {weekdayLabelFor(date)} {dayFormatter.format(parseIsoDate(date))}
                  {!isEligible && <span className="text-[10px]">({reason})</span>}
                  {failedDays.includes(date) && <span className="text-[10px] text-danger-400">błąd</span>}
                </label>
              </li>
            )
          })}
        </ul>

        <div className="flex flex-wrap items-center gap-3">
          <button
            type="button"
            onClick={() => setStatus('Available')}
            className={`rounded-md border px-3 py-1 text-xs ${
              status === 'Available' ? 'border-success-700 bg-success-950 text-success-300' : 'border-neutral-700 text-neutral-400'
            }`}
          >
            Dostępny
          </button>
          <button
            type="button"
            onClick={() => setStatus('Off')}
            className={`rounded-md border px-3 py-1 text-xs ${
              status === 'Off' ? 'border-neutral-400 bg-neutral-800 text-neutral-100' : 'border-neutral-700 text-neutral-400'
            }`}
          >
            Nie gram
          </button>
          <label className="flex items-center gap-1.5 text-xs text-neutral-300 has-[:disabled]:opacity-50">
            <input type="checkbox" checked={fullDay} disabled={status === 'Off'} onChange={(e) => setFullDay(e.target.checked)} />
            Cały dzień
          </label>
        </div>

        {isPartial && <TimeRangePicker from={from} to={to} onChange={(range) => { setFrom(range.from); setTo(range.to) }} />}

        {error !== null && <p className="text-sm text-danger-400">{error}</p>}

        <div className="flex gap-2">
          <button
            type="button"
            disabled={isSaving}
            onClick={() => void apply()}
            className="rounded-md bg-primary-500 px-3 py-1.5 text-xs font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
          >
            Zastosuj
          </button>
          <button type="button" onClick={onClose} className="rounded-md border border-neutral-700 px-3 py-1.5 text-xs text-neutral-300">
            Anuluj
          </button>
        </div>
      </div>
    </Modal>
  )
}
