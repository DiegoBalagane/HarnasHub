import { memo, useMemo } from 'react'
import type { CalendarEvent } from '../../../../services/calendarApi'
import { weekdayLabels } from '../../../availability/labels'
import { parseIsoDate } from '../../../availability/weekDates'
import { buildMonthMatrix, groupEventsByDate } from '../../calendarGrid'
import { EventChip } from './EventChip'

/** Chips shown per day cell before collapsing into "+N więcej". */
const maxChipsPerDay = 2

interface MonthViewProps {
  anchorIso: string
  todayIso: string
  events: readonly CalendarEvent[]
  /** Set for Coach/Manager: clicking an empty part of a day creates an event on it. */
  onCreateOnDay?: (dateIso: string) => void
  onSelectEvent: (eventId: string) => void
  /** Opens the given day in the week view (used by "+N więcej"). */
  onOpenDay: (dateIso: string) => void
}

/** Month grid that fills its parent's height: Monday-first rows, type-coloured chips, "+N więcej" overflow. */
export const MonthView = memo(function MonthView({
  anchorIso,
  todayIso,
  events,
  onCreateOnDay,
  onSelectEvent,
  onOpenDay,
}: MonthViewProps) {
  const weeks = useMemo(() => buildMonthMatrix(anchorIso), [anchorIso])
  const byDate = useMemo(() => groupEventsByDate(events), [events])
  const month = anchorIso.slice(0, 7)

  return (
    <div className="flex min-h-[26rem] flex-1 flex-col overflow-hidden rounded-md border border-surface-border">
      <div className="grid grid-cols-7 border-b border-surface-border bg-surface-card text-center text-[11px] text-neutral-400">
        {weekdayLabels.map((label) => (
          <div key={label} className="py-1.5">
            {label}
          </div>
        ))}
      </div>
      <div
        className="grid min-h-0 flex-1 grid-cols-7"
        style={{ gridTemplateRows: `repeat(${weeks.length}, minmax(0, 1fr))` }}
      >
        {weeks.flat().map((date) => {
          const dayEvents = byDate.get(date) ?? []
          const hidden = dayEvents.length - maxChipsPerDay
          const isToday = date === todayIso
          const inMonth = date.startsWith(month)

          return (
            <div
              key={date}
              role="gridcell"
              aria-label={date}
              onClick={onCreateOnDay ? () => onCreateOnDay(date) : undefined}
              className={`flex min-h-0 min-w-0 flex-col gap-0.5 overflow-hidden border-b border-r border-surface-border p-1 ${
                inMonth ? '' : 'bg-neutral-950/60'
              } ${onCreateOnDay ? 'cursor-pointer hover:bg-neutral-900' : ''}`}
            >
              <span
                className={`flex h-5 w-5 shrink-0 items-center justify-center rounded-full text-[11px] ${
                  isToday
                    ? 'bg-primary-500 font-semibold text-primary-950'
                    : inMonth
                      ? 'text-neutral-300'
                      : 'text-neutral-600'
                }`}
              >
                {parseIsoDate(date).getDate()}
              </span>
              {dayEvents.slice(0, maxChipsPerDay).map((event) => (
                <EventChip key={event.id} event={event} onSelect={onSelectEvent} />
              ))}
              {hidden > 0 && (
                <button
                  type="button"
                  onClick={(clickEvent) => {
                    clickEvent.stopPropagation()
                    onOpenDay(date)
                  }}
                  className="truncate text-left text-[10px] text-neutral-400 hover:text-white"
                >
                  +{hidden} więcej
                </button>
              )}
            </div>
          )
        })}
      </div>
    </div>
  )
})
