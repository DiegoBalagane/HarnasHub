import React from 'react'
import { Link } from 'react-router-dom'
import type { CalendarEvent } from '../../../../services/calendarApi'
import type { DaySummary } from '../../daySummary'
import { parseIsoDate, weekdayLabelFor } from '../../weekDates'
import { describeDay, fillTone, shortHour, type FillTone } from '../../weekModel'

const dayFormatter = new Intl.DateTimeFormat('pl-PL', { day: '2-digit', month: '2-digit' })
const barClass: Record<FillTone, string> = {
  success: 'bg-success-500',
  warning: 'bg-warning-500',
  danger: 'bg-danger-500',
  none: 'bg-neutral-700',
}
const matchLikeTypes = new Set(['Match', 'Tournament', 'Scrim'])

interface DayHeaderProps {
  date: string
  isToday: boolean
  summary: DaySummary
  events: CalendarEvent[]
  absentNames: string[]
}

/** Two-line day header: "Śr 07.10" with event dots, then a main-roster fill bar with "5/5" and the common hour window. */
export const DayHeader = React.memo(function DayHeader({ date, isToday, summary, events, absentNames }: DayHeaderProps) {
  const { availableCount, totalCount } = summary.main
  const tone = fillTone(availableCount, totalCount)
  const percent = totalCount > 0 ? Math.round((availableCount / totalCount) * 100) : 0
  const tooltip = describeDay(summary, absentNames, events.map((event) => event.title))
  const commonWindow = summary.commonWindow

  return (
    <div
      title={tooltip}
      className={`flex flex-col justify-center gap-1 rounded-md px-2 py-1 ${
        isToday ? 'bg-primary-500/10 ring-1 ring-primary-500/60' : 'bg-neutral-900/60'
      }`}
    >
      <div className="flex items-center justify-between gap-1 text-xs">
        <span className={`font-medium ${isToday ? 'text-primary-300' : 'text-neutral-200'}`}>
          {weekdayLabelFor(date)} <span className="text-neutral-500">{dayFormatter.format(parseIsoDate(date))}</span>
        </span>
        <span className="flex items-center gap-1">
          {events.map((event) => (
            <Link
              key={event.id}
              to={`/calendar?event=${event.id}`}
              aria-label={`Wydarzenie: ${event.title}`}
              title={event.title}
              className={`h-2 w-2 rounded-full ${matchLikeTypes.has(event.type) ? 'bg-primary-400' : 'bg-info-400'}`}
            />
          ))}
        </span>
      </div>
      <div className="flex items-center gap-1.5 text-[11px]">
        <div
          role="progressbar"
          aria-label="Wypełnienie głównego składu"
          aria-valuemin={0}
          aria-valuemax={totalCount}
          aria-valuenow={availableCount}
          className="h-1.5 flex-1 overflow-hidden rounded-full bg-neutral-800"
        >
          <div className={`h-full ${barClass[tone]}`} style={{ width: `${percent}%` }} />
        </div>
        <span className="font-medium text-neutral-300">
          {availableCount}/{totalCount}
        </span>
        {commonWindow && (
          <span className="text-neutral-500">
            {shortHour(commonWindow.from)}–{shortHour(commonWindow.to)}
          </span>
        )}
      </div>
    </div>
  )
})
