import type { DayEntry, MemberWeek } from '../../services/availabilityApi'

const daysInWeek = 7

/** Returns the member's entry for one day, or undefined when the server row is incomplete. */
export function entryFor(member: MemberWeek, date: string): DayEntry | undefined {
  return member.days.find((day) => day.date === date)
}

/** Sort weight for a roster slot: Main first, then Bench, then anyone unassigned. */
export function rosterSlotRank(rosterSlot: string | null): number {
  if (rosterSlot === 'Main') return 0
  if (rosterSlot === 'Bench') return 1
  return 2
}

/** Formats a Date as a local yyyy-MM-dd string (unlike toISOString, which shifts to UTC). */
export function toIsoDate(date: Date): string {
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${date.getFullYear()}-${month}-${day}`
}

/** Parses a yyyy-MM-dd string into a local-midnight Date. */
export function parseIsoDate(iso: string): Date {
  const [year, month, day] = iso.split('-').map(Number)
  return new Date(year, month - 1, day)
}

/** Returns the yyyy-MM-dd of the Monday starting the week that contains the given date. */
export function getWeekStartIso(date: Date): string {
  const start = new Date(date.getFullYear(), date.getMonth(), date.getDate())
  const mondayOffset = (start.getDay() + 6) % daysInWeek
  start.setDate(start.getDate() - mondayOffset)
  return toIsoDate(start)
}

/** Shifts a yyyy-MM-dd string by whole days. */
export function addDaysIso(iso: string, days: number): string {
  const date = parseIsoDate(iso)
  date.setDate(date.getDate() + days)
  return toIsoDate(date)
}

/** Expands a week start into its seven yyyy-MM-dd days, Monday first. */
export function buildWeekDates(weekStartIso: string): string[] {
  return Array.from({ length: daysInWeek }, (_, offset) => addDaysIso(weekStartIso, offset))
}

/** Trims a server HH:mm:ss time to the HH:mm form used by inputs and badges. */
export function toShortTime(time: string | null): string | null {
  return time === null ? null : time.slice(0, 5)
}

/** Builds up to two uppercase initials from a display name. */
export function toInitials(displayName: string): string {
  return displayName
    .split(/\s+/)
    .filter((part) => part.length > 0)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('')
}
