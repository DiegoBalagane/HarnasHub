import type { MemberWeek } from '../../services/availabilityApi'
import { entryFor, parseIsoDate } from './weekDates'

export type BulkChip = 'all' | 'workdays' | 'weekend'

/** True for Saturday and Sunday. */
export function isWeekend(date: string): boolean {
  const day = parseIsoDate(date).getDay()
  return day === 0 || day === 6
}

/** Days of the shown week the member may bulk-edit: not in the past and not covered by a vacation. */
export function eligibleBulkDays(member: MemberWeek, dates: string[], todayIso: string): string[] {
  return dates.filter((date) => {
    const entry = entryFor(member, date)
    return date >= todayIso && entry !== undefined && !entry.isVacation
  })
}

/** Selection produced by a quick chip, limited to eligible days. */
export function chipSelection(chip: BulkChip, eligible: string[]): string[] {
  if (chip === 'workdays') return eligible.filter((date) => !isWeekend(date))
  if (chip === 'weekend') return eligible.filter(isWeekend)
  return eligible
}
