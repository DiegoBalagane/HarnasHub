import type { MemberWeek } from '../../services/availabilityApi'
import { entryFor } from './weekDates'

export interface DaySummary {
  /** Members declared Available or PartiallyAvailable that day, out of the whole roster shown in the grid. */
  availableCount: number
  totalCount: number
  /** Narrowest HH:mm window every PartiallyAvailable member has in common, or null when nobody restricted their hours or the windows don't overlap. */
  commonWindow: { from: string; to: string } | null
}

/** Summarizes one day across the whole roster: how many could show up, and the hour range they'd all overlap in. */
export function computeDaySummary(members: MemberWeek[], date: string): DaySummary {
  let availableCount = 0
  let commonFrom: string | null = null
  let commonTo: string | null = null

  for (const member of members) {
    const entry = entryFor(member, date)

    if (!entry || (entry.status !== 'Available' && entry.status !== 'PartiallyAvailable')) {
      continue
    }

    availableCount += 1

    if (entry.status === 'PartiallyAvailable' && entry.from && entry.to) {
      if (commonFrom === null || entry.from > commonFrom) {
        commonFrom = entry.from
      }
      if (commonTo === null || entry.to < commonTo) {
        commonTo = entry.to
      }
    }
  }

  const commonWindow =
    commonFrom !== null && commonTo !== null && commonFrom < commonTo
      ? { from: commonFrom.slice(0, 5), to: commonTo.slice(0, 5) }
      : null

  return { availableCount, totalCount: members.length, commonWindow }
}
