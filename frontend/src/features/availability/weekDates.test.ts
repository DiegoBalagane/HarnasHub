import { describe, expect, it } from 'vitest'
import { addDaysIso, buildWeekDates, rosterSlotRank, toInitials, toIsoDate, toShortTime, weekdayLabelFor } from './weekDates'

describe('weekDates helpers', () => {
  it('shifts dates across month and year boundaries', () => {
    expect(addDaysIso('2026-12-30', 3)).toBe('2027-01-02')
    expect(addDaysIso('2026-03-01', -1)).toBe('2026-02-28')
  })

  it('builds seven consecutive days', () => {
    const days = buildWeekDates('2026-10-05')
    expect(days).toHaveLength(7)
    expect(days[6]).toBe('2026-10-11')
  })

  it('formats a local date without UTC shift', () => {
    expect(toIsoDate(new Date(2026, 0, 5, 0, 30))).toBe('2026-01-05')
  })

  it('labels weekdays Monday-first by real day of week', () => {
    expect(weekdayLabelFor('2026-10-05')).toBe('Pon')
    expect(weekdayLabelFor('2026-10-11')).toBe('Nd')
  })

  it('ranks Main before Bench before unassigned', () => {
    expect([null, 'Bench', 'Main'].sort((a, b) => rosterSlotRank(a) - rosterSlotRank(b))).toEqual(['Main', 'Bench', null])
  })

  it('trims times and builds initials', () => {
    expect(toShortTime('18:30:00')).toBe('18:30')
    expect(toShortTime(null)).toBeNull()
    expect(toInitials('  jan  kowalski nowak')).toBe('JK')
  })
})
