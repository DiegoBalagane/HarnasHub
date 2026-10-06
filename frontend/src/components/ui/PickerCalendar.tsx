import { useEffect, useMemo, useRef } from 'react'
import { addMonthsIso, buildMonthMatrix, startOfMonthIso } from '../../features/calendar/calendarGrid'
import { addDaysIso, parseIsoDate, toIsoDate } from '../../features/availability/weekDates'

const monthTitle = new Intl.DateTimeFormat('pl-PL', { month: 'long', year: 'numeric' })
const dayLabel = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'full' })
const weekdayHeaders = ['Pn', 'Wt', 'Śr', 'Cz', 'Pt', 'So', 'Nd']

/** Same day-of-month in another month, clamped to that month's last day. */
function shiftMonthKeepingDay(iso: string, months: number): string {
  const first = parseIsoDate(addMonthsIso(iso, months))
  const lastDay = new Date(first.getFullYear(), first.getMonth() + 1, 0).getDate()
  first.setDate(Math.min(Number(iso.slice(8)), lastDay))
  return toIsoDate(first)
}

interface PickerCalendarProps {
  /** Selected day (`yyyy-MM-dd`) or '' when none. */
  selected: string
  /** Day that holds the keyboard focus and decides the shown month. */
  focused: string
  today: string
  onFocusedChange: (iso: string) => void
  onSelect: (iso: string) => void
}

/** Month grid of the picker popover: Polish locale, Monday first, today ringed, arrow/PageUp/PageDown/Home navigation. */
export function PickerCalendar({ selected, focused, today, onFocusedChange, onSelect }: PickerCalendarProps) {
  const weeks = useMemo(() => buildMonthMatrix(focused), [focused])
  const gridRef = useRef<HTMLDivElement>(null)
  const shouldFocusRef = useRef(false)
  const month = focused.slice(0, 7)

  useEffect(() => {
    if (!shouldFocusRef.current) return
    shouldFocusRef.current = false
    gridRef.current?.querySelector<HTMLButtonElement>(`[data-date="${focused}"]`)?.focus()
  }, [focused])

  function handleKeyDown(event: React.KeyboardEvent) {
    const steps: Record<string, string> = {
      ArrowLeft: addDaysIso(focused, -1),
      ArrowRight: addDaysIso(focused, 1),
      ArrowUp: addDaysIso(focused, -7),
      ArrowDown: addDaysIso(focused, 7),
      PageUp: shiftMonthKeepingDay(focused, -1),
      PageDown: shiftMonthKeepingDay(focused, 1),
      Home: startOfMonthIso(focused),
    }
    const next = steps[event.key]
    if (!next) return
    event.preventDefault()
    shouldFocusRef.current = true
    onFocusedChange(next)
  }

  return (
    <div>
      <div className="mb-2 flex items-center justify-between">
        <button
          type="button"
          aria-label="Poprzedni miesiąc"
          onClick={() => onFocusedChange(shiftMonthKeepingDay(focused, -1))}
          className="rounded px-3 py-1 text-neutral-300 hover:bg-neutral-800"
        >
          ‹
        </button>
        <span aria-live="polite" className="text-sm font-medium capitalize text-neutral-100">
          {monthTitle.format(parseIsoDate(focused))}
        </span>
        <button
          type="button"
          aria-label="Następny miesiąc"
          onClick={() => onFocusedChange(shiftMonthKeepingDay(focused, 1))}
          className="rounded px-3 py-1 text-neutral-300 hover:bg-neutral-800"
        >
          ›
        </button>
      </div>

      <div ref={gridRef} role="grid" aria-label="Kalendarz" onKeyDown={handleKeyDown} className="flex flex-col gap-0.5">
        <div role="row" className="grid grid-cols-7 text-center text-[11px] text-neutral-500">
          {weekdayHeaders.map((header) => (
            <span key={header} role="columnheader" className="py-1">
              {header}
            </span>
          ))}
        </div>
        {weeks.map((week) => (
          <div key={week[0]} role="row" className="grid grid-cols-7">
            {week.map((iso) => {
              const isSelected = iso === selected
              const isToday = iso === today
              const inMonth = iso.slice(0, 7) === month
              const tone = isSelected
                ? 'bg-primary-500 font-semibold text-primary-950'
                : isToday
                  ? 'border border-primary-500 text-primary-300 hover:bg-neutral-800'
                  : inMonth
                    ? 'text-neutral-200 hover:bg-neutral-800'
                    : 'text-neutral-600 hover:bg-neutral-800'
              return (
                <div key={iso} role="gridcell" className="flex justify-center">
                  <button
                    type="button"
                    data-date={iso}
                    tabIndex={iso === focused ? 0 : -1}
                    aria-label={dayLabel.format(parseIsoDate(iso))}
                    aria-pressed={isSelected}
                    aria-current={isToday ? 'date' : undefined}
                    onClick={() => onSelect(iso)}
                    className={`h-9 w-9 rounded-full text-sm transition ${tone}`}
                  >
                    {Number(iso.slice(8))}
                  </button>
                </div>
              )
            })}
          </div>
        ))}
      </div>
    </div>
  )
}
