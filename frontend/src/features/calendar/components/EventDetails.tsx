import type { CalendarEvent } from '../../../services/calendarApi'
import { GamePlanSection } from '../../game-plan/components/GamePlanSection'
import { EventVetoSection } from '../../veto/components/EventVetoSection'
import { AvailabilityPicker } from './AvailabilityPicker'

/** Expanded body of an event: availability for everyone, the game plan, and the veto when there's an opponent. */
export function EventDetails({ event }: { event: CalendarEvent }) {
  return (
    <div className="mt-3 flex flex-col gap-3">
      <AvailabilityPicker eventId={event.id} />
      <GamePlanSection eventId={event.id} title={event.title} />
      {event.opponent && <EventVetoSection eventId={event.id} opponent={event.opponent} />}
    </div>
  )
}
