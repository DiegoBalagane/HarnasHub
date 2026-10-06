import { dayStatusColors, dayStatusLabels, vacationColor, vacationIcon, vacationLabel } from '../labels'

const legendStatuses = ['Available', 'PartiallyAvailable', 'Off', 'NotSet'] as const

/** Colour legend for the availability grid statuses. */
export function AvailabilityLegend() {
  return (
    <ul className="flex flex-wrap items-center gap-x-4 gap-y-1 text-[11px] text-neutral-400" aria-label="Legenda">
      {legendStatuses.map((status) => (
        <li key={status} className="flex items-center gap-1.5">
          <span className={`inline-block h-3 w-5 rounded border ${dayStatusColors[status]}`} />
          {dayStatusLabels[status]}
        </li>
      ))}
      <li className="flex items-center gap-1.5">
        <span className={`inline-block h-3 w-5 rounded border ${vacationColor}`} />
        {vacationIcon} {vacationLabel}
      </li>
    </ul>
  )
}
