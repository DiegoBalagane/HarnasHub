import { describe, expect, it } from 'vitest'
import type { DayEntry, MemberWeek } from '../../services/availabilityApi'
import { computeDaySummary } from './daySummary'

const date = '2026-10-06'

function member(rosterSlot: string | null, entry: Partial<DayEntry> | null): MemberWeek {
  return {
    userId: String(Math.random()),
    displayName: 'P',
    rosterSlot,
    days: entry ? [{ date, status: 'NotSet', from: null, to: null, isVacation: false, note: null, ...entry }] : [],
  } as unknown as MemberWeek
}

describe('computeDaySummary', () => {
  it('splits counts between Main and the rest', () => {
    const summary = computeDaySummary(
      [
        member('Main', { status: 'Available' }),
        member('Main', { status: 'Off' }),
        member('Bench', { status: 'Available' }),
        member(null, null),
      ],
      date,
    )
    expect(summary.main).toEqual({ availableCount: 1, totalCount: 2 })
    expect(summary.rest).toEqual({ availableCount: 1, totalCount: 2 })
    expect(summary.commonWindow).toBeNull()
  })

  it('intersects partially available windows and trims seconds', () => {
    const summary = computeDaySummary(
      [
        member('Main', { status: 'PartiallyAvailable', from: '17:00:00', to: '22:00:00' }),
        member('Main', { status: 'PartiallyAvailable', from: '18:30:00', to: '23:00:00' }),
        member('Main', { status: 'Available' }),
      ],
      date,
    )
    expect(summary.commonWindow).toEqual({ from: '18:30', to: '22:00' })
    expect(summary.main.availableCount).toBe(3)
  })

  it('returns no window when the windows do not overlap', () => {
    const summary = computeDaySummary(
      [
        member('Main', { status: 'PartiallyAvailable', from: '10:00:00', to: '12:00:00' }),
        member('Main', { status: 'PartiallyAvailable', from: '18:00:00', to: '20:00:00' }),
      ],
      date,
    )
    expect(summary.commonWindow).toBeNull()
  })

  it('ignores the viewer\'s own row when they are hidden from the calendar', () => {
    const hidden = { ...member('Main', { status: 'Available' }), hiddenFromCalendar: true }
    const summary = computeDaySummary([hidden, member('Main', { status: 'Available' })], date)

    expect(summary.main).toEqual({ availableCount: 1, totalCount: 1 })
  })
})
