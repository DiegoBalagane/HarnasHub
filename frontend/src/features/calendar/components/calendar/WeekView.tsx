import { memo, useMemo } from 'react'
import type { CalendarEvent } from '../../../../services/calendarApi'
import { parseIsoDate, toIsoDate, weekdayLabelFor } from '../../../availability/weekDates'
import { groupEventsByDate, layoutDayEvents, weekDaysOf, weekHourRange } from '../../calendarGrid'
import { EventChip } from './EventChip'

/** Height of one hour row; the grid scrolls inside its own box when the range is taller than the viewport. */
const hourHeightPx = 36
const pxPerMinute = hourHeightPx / 60
const dayFormatter = new Intl.DateTimeFormat('pl-PL', { day: 'numeric', month: 'numeric' })
const gridColumns = 'grid grid-cols-[2.5rem_repeat(7,minmax(0,1fr))]'

/** Per-day "available / total" count of the main roster, shown in the day header when known. */
export type DayAvailabilityCounts = Record<string, { available: number; total: number }>

interface WeekViewProps {
  anchorIso: string
  todayIso: string
  events: readonly CalendarEvent[]
  availability?: DayAvailabilityCounts
  /** Set for Coach/Manager: clicking an empty hour slot creates an event starting then. */
  onCreateAt?: (dateIso: string, hour: number) => void
  onSelectEvent: (eventId: string) => void
}

/** Week time grid: Monday-first day columns, hour rows (8-24, widened for earlier events), events as positioned blocks. */
export const WeekView = memo(function WeekView({
  anchorIso,
  todayIso,
  events,
  availability,
  onCreateAt,
  onSelectEvent,
}: WeekViewProps) {
  const days = useMemo(() => weekDaysOf(anchorIso), [anchorIso])
  const weekEvents = useMemo(
    () => events.filter((event) => days.includes(toIsoDate(new Date(event.startsAtUtc)))),
    [events, days],
  )
  const byDate = useMemo(() => groupEventsByDate(weekEvents), [weekEvents])
  const { startHour, endHour } = useMemo(() => weekHourRange(weekEvents), [weekEvents])
  const hours = Array.from({ length: endHour - startHour }, (_, index) => startHour + index)

  return (
    <div className="flex min-h-[26rem] flex-1 flex-col overflow-hidden rounded-md border border-surface-border">
      <div className={`${gridColumns} border-b border-surface-border bg-surface-card`}>
        <div />
        {days.map((date) => {
          const counts = availability?.[date]

          return (
            <div key={date} className={`px-1 py-1.5 text-center ${date === todayIso ? 'bg-primary-500/10' : ''}`}>
              <p className="text-[11px] text-neutral-400">{weekdayLabelFor(date)}</p>
              <p className={`text-sm font-medium ${date === todayIso ? 'text-primary-400' : 'text-neutral-200'}`}>
                {dayFormatter.format(parseIsoDate(date))}
              </p>
              {counts && counts.total > 0 && (
                <p
                  className={`truncate text-[10px] ${counts.available === counts.total ? 'text-success-400' : 'text-neutral-500'}`}
                >
                  {counts.available}/{counts.total} dostępnych
                </p>
              )}
            </div>
          )
        })}
      </div>

      <div className="min-h-0 flex-1 overflow-y-auto">
        <div className={gridColumns} style={{ height: hours.length * hourHeightPx }}>
          <div>
            {hours.map((hour) => (
              <div key={hour} style={{ height: hourHeightPx }} className="pr-1 text-right text-[10px] text-neutral-500">
                {String(hour).padStart(2, '0')}:00
              </div>
            ))}
          </div>
          {days.map((date) => (
            <div
              key={date}
              className={`relative border-l border-surface-border ${date === todayIso ? 'bg-primary-500/5' : ''}`}
            >
              {hours.map((hour) =>
                onCreateAt ? (
                  <button
                    key={hour}
                    type="button"
                    aria-label={`Dodaj wydarzenie ${date} ${String(hour).padStart(2, '0')}:00`}
                    onClick={() => onCreateAt(date, hour)}
                    style={{ height: hourHeightPx }}
                    className="block w-full border-t border-surface-border/70 hover:bg-neutral-900"
                  />
                ) : (
                  <div key={hour} style={{ height: hourHeightPx }} className="border-t border-surface-border/70" />
                ),
              )}
              {layoutDayEvents(byDate.get(date) ?? [], startHour).map((placed) => (
                <div
                  key={placed.event.id}
                  className="absolute px-px"
                  style={{
                    top: placed.topMinutes * pxPerMinute,
                    height: placed.heightMinutes * pxPerMinute,
                    left: `${(placed.column / placed.columns) * 100}%`,
                    width: `${100 / placed.columns}%`,
                  }}
                >
                  <EventChip
                    event={placed.event}
                    onSelect={onSelectEvent}
                    className="h-full items-start whitespace-normal"
                  />
                </div>
              ))}
            </div>
          ))}
        </div>
      </div>
    </div>
  )
})
