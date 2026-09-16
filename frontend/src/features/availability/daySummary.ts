import type { MemberWeek } from '../../services/availabilityApi'
import { entryFor } from './weekDates'

export interface RosterGroupSummary {
  availableCount: number
  totalCount: number
}

export interface DaySummary {
  /** Main-roster members declared Available or PartiallyAvailable that day, out of the main roster shown in the grid. */
  main: RosterGroupSummary
  /** Everyone else in the grid (bench + unassigned) — StandIn members never appear here at all. */
  rest: RosterGroupSummary
  /** Narrowest HH:mm window every PartiallyAvailable member has in common, or null when nobody restricted their hours or the windows don't overlap. */
  commonWindow: { from: string; to: string } | null
}

/** Summarizes one day across the whole roster, split into Main vs the rest of the team, plus the hour range they'd all overlap in. */
export function computeDaySummary(members: MemberWeek[], date: string): DaySummary {
  const main: RosterGroupSummary = { availableCount: 0, totalCount: 0 }
  const rest: RosterGroupSummary = { availableCount: 0, totalCount: 0 }
  let commonFrom: string | null = null
  let commonTo: string | null = null

  for (const member of members) {
    const group = member.rosterSlot === 'Main' ? main : rest
    group.totalCount += 1

    const entry = entryFor(member, date)

    if (!entry || (entry.status !== 'Available' && entry.status !== 'PartiallyAvailable')) {
      continue
    }

    group.availableCount += 1

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

  return { main, rest, commonWindow }
}
