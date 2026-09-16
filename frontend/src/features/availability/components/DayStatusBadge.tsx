import React from 'react'
import type { DayStatusView } from '../../../services/availabilityApi'
import { dayStatusColors, dayStatusLabels, vacationColor, vacationIcon, vacationLabel } from '../labels'
import { toShortTime } from '../weekDates'

/** Renders one member's effective status for one day as a compact colored badge. */
export const DayStatusBadge = React.memo(function DayStatusBadge({ entry }: { entry: DayStatusView }) {
  const from = toShortTime(entry.from)
  const to = toShortTime(entry.to)

  const color = entry.isVacation ? vacationColor : dayStatusColors[entry.status]
  const text = entry.isVacation
    ? `${vacationIcon} ${vacationLabel}`
    : entry.status === 'PartiallyAvailable' && from !== null && to !== null
      ? `${from}–${to}`
      : entry.status === 'NotSet'
        ? '—'
        : dayStatusLabels[entry.status]

  return (
    <span
      title={entry.note ?? dayStatusLabels[entry.status]}
      className={`block truncate rounded-md border px-2 py-1 text-center text-xs ${color}`}
    >
      {text}
    </span>
  )
})
