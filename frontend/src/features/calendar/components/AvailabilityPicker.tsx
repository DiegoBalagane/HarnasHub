import type { AvailabilityStatus } from '../../../services/calendarApi'
import { useEventAvailability, useSetAvailability } from '../hooks/useCalendar'
import { availabilityColors, availabilityLabels } from '../labels'

const statuses: AvailabilityStatus[] = ['Available', 'Maybe', 'Unavailable']

/** Shows every team member's availability for an event and lets the viewer set their own. */
export function AvailabilityPicker({ eventId }: { eventId: string }) {
  const { data: members, isLoading } = useEventAvailability(eventId, true)
  const setAvailability = useSetAvailability(eventId)

  return (
    <div className="flex flex-col gap-3 border-t border-neutral-800 pt-3">
      <div className="flex gap-2">
        {statuses.map((status) => (
          <button
            key={status}
            onClick={() => setAvailability.mutate(status)}
            disabled={setAvailability.isPending}
            className="rounded-md border border-neutral-700 px-3 py-1 text-xs hover:border-neutral-500 disabled:opacity-50"
          >
            {availabilityLabels[status]}
          </button>
        ))}
      </div>

      {isLoading ? (
        <p className="text-xs text-neutral-500">Ładowanie dostępności…</p>
      ) : (
        <ul className="flex flex-col gap-1 text-xs">
          {members?.map((member) => (
            <li key={member.userId} className="flex justify-between">
              <span className="text-neutral-300">{member.inGameNickname ?? member.displayName}</span>
              <span className={availabilityColors[member.status]}>{availabilityLabels[member.status]}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
