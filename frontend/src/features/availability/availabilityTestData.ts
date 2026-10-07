import type { DayEntry, MemberWeek } from '../../services/availabilityApi'
import { buildWeekDates } from './weekDates'

export const testToday = '2026-10-07'

/** Builds a member with a 7-day week starting at `testToday`; `overrides` patches single days by date. */
export function makeMember(
  userId: string,
  name: string,
  options: { start?: string; rosterSlot?: string | null; isCoach?: boolean; hidden?: boolean; base?: Partial<DayEntry>; overrides?: Record<string, Partial<DayEntry>> } = {},
): MemberWeek {
  const days: DayEntry[] = buildWeekDates(options.start ?? testToday).map((date) => ({
    date,
    status: 'NotSet',
    from: null,
    to: null,
    isVacation: false,
    note: null,
    ...options.base,
    ...options.overrides?.[date],
  }))

  return {
    userId,
    displayName: name,
    inGameNickname: name,
    teamRole: null,
    rosterSlot: options.rosterSlot === undefined ? 'Main' : options.rosterSlot,
    isCoach: options.isCoach ?? false,
    days,
    hiddenFromCalendar: options.hidden,
  }
}
