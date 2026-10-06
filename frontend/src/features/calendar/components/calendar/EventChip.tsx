import { memo } from 'react'
import type { CalendarEvent } from '../../../../services/calendarApi'
import { eventTypeBlockColors, eventTypeColors, eventTypeLabels } from '../../labels'

const timeFormatter = new Intl.DateTimeFormat('pl-PL', { timeStyle: 'short' })

/** Display caption of an event: matches show the opponent, everything else its title. */
export function eventCaption(event: CalendarEvent): string {
  return event.type === 'Match' && event.opponent ? `vs ${event.opponent}` : event.title
}

interface EventChipProps {
  event: CalendarEvent
  onSelect: (eventId: string) => void
  /** Show the start time before the caption (month/agenda). */
  showTime?: boolean
  className?: string
}

/** Compact event button tinted by event type; match events show the opponent. */
export const EventChip = memo(function EventChip({ event, onSelect, showTime = true, className = '' }: EventChipProps) {
  const time = timeFormatter.format(new Date(event.startsAtUtc))

  return (
    <button
      type="button"
      title={`${eventTypeLabels[event.type]}: ${event.title}`}
      onClick={(clickEvent) => {
        clickEvent.stopPropagation()
        onSelect(event.id)
      }}
      className={`flex w-full min-w-0 items-center gap-1 truncate rounded border px-1.5 py-0.5 text-left text-[11px] leading-tight transition ${eventTypeBlockColors[event.type]} ${className}`}
    >
      {showTime && <span className={`shrink-0 font-medium ${eventTypeColors[event.type]}`}>{time}</span>}
      <span className="truncate text-neutral-100">{eventCaption(event)}</span>
    </button>
  )
})
