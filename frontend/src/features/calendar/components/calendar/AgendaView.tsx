import { memo, useMemo } from 'react'
import type { CalendarEvent } from '../../../../services/calendarApi'
import { parseIsoDate } from '../../../availability/weekDates'
import { groupEventsByDate } from '../../calendarGrid'
import { eventTypeColors, eventTypeLabels } from '../../labels'
import { EventChip } from './EventChip'

const dayFormatter = new Intl.DateTimeFormat('pl-PL', { weekday: 'long', day: 'numeric', month: 'long' })

interface AgendaViewProps {
  anchorIso: string
  todayIso: string
  events: readonly CalendarEvent[]
  onSelectEvent: (eventId: string) => void
}

/** Vertical list of the month's event days (past days hidden in the current month), best for phones. */
export const AgendaView = memo(function AgendaView({ anchorIso, todayIso, events, onSelectEvent }: AgendaViewProps) {
  const days = useMemo(() => {
    const month = anchorIso.slice(0, 7)
    const hidePast = todayIso.startsWith(month)

    return [...groupEventsByDate(events)]
      .filter(([date]) => date.startsWith(month) && (!hidePast || date >= todayIso))
      .sort(([left], [right]) => left.localeCompare(right))
  }, [anchorIso, todayIso, events])

  if (days.length === 0) {
    return <p className="py-8 text-center text-sm text-neutral-400">Brak wydarzeń w tym okresie.</p>
  }

  return (
    <ul className="flex flex-col gap-4">
      {days.map(([date, dayEvents]) => (
        <li key={date} className="flex flex-col gap-1.5">
          <h3 className={`text-xs font-medium capitalize ${date === todayIso ? 'text-primary-400' : 'text-neutral-400'}`}>
            {date === todayIso ? 'Dziś · ' : ''}
            {dayFormatter.format(parseIsoDate(date))}
          </h3>
          {dayEvents.map((event) => (
            <div key={event.id} className="flex flex-col gap-0.5">
              <EventChip event={event} onSelect={onSelectEvent} className="py-2 text-sm" />
              <p className="px-1 text-[11px] text-neutral-500">
                <span className={eventTypeColors[event.type]}>{eventTypeLabels[event.type]}</span>
                {event.location ? ` · ${event.location}` : ''}
              </p>
            </div>
          ))}
        </li>
      ))}
    </ul>
  )
})
