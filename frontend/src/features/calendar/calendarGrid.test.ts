import { describe, expect, it } from 'vitest'
import type { CalendarEvent } from '../../services/calendarApi'
import {
  addMonthsIso,
  buildMonthMatrix,
  formatRangeLabel,
  layoutDayEvents,
  shiftAnchor,
  startOfWeekIso,
  weekDaysOf,
  weekHourRange,
} from './calendarGrid'

function eventAt(id: string, startLocal: string, endLocal: string | null = null): CalendarEvent {
  return {
    id,
    title: id,
    type: 'Training',
    startsAtUtc: new Date(startLocal).toISOString(),
    endsAtUtc: endLocal ? new Date(endLocal).toISOString() : null,
    location: null,
    url: null,
    notes: null,
    opponent: null,
  }
}

describe('calendarGrid', () => {
  it('starts weeks on Monday', () => {
    expect(startOfWeekIso('2026-10-07')).toBe('2026-10-05')
    expect(startOfWeekIso('2026-10-11')).toBe('2026-10-05')
    expect(weekDaysOf('2026-10-05')).toHaveLength(7)
  })

  it('builds a month matrix padded to full Monday-first weeks', () => {
    const weeks = buildMonthMatrix('2026-10-15')
    expect(weeks[0][0]).toBe('2026-09-28')
    expect(weeks[0][3]).toBe('2026-10-01')
    expect(weeks.at(-1)?.[6]).toBe('2026-11-01')
    expect(weeks.every((week) => week.length === 7)).toBe(true)
  })

  it('covers month boundaries (Feb 2027 starts on Monday, 4 rows; Mar 2026 needs 6)', () => {
    expect(buildMonthMatrix('2027-02-10')).toHaveLength(4)
    expect(buildMonthMatrix('2027-02-10')[0][0]).toBe('2027-02-01')
    expect(buildMonthMatrix('2026-03-01')).toHaveLength(6)
  })

  it('shifts months and weeks', () => {
    expect(addMonthsIso('2026-01-31', 1)).toBe('2026-02-01')
    expect(shiftAnchor('2026-12-15', 'month', 1)).toBe('2027-01-01')
    expect(shiftAnchor('2026-10-07', 'week', -1)).toBe('2026-09-30')
  })

  it('formats a Polish range label', () => {
    expect(formatRangeLabel('2026-10-07', 'month')).toBe('październik 2026')
    expect(formatRangeLabel('2026-10-07', 'week')).toContain('5 – 11 października 2026')
  })

  it('uses 8-24 by default and widens for early events', () => {
    expect(weekHourRange([])).toEqual({ startHour: 8, endHour: 24 })
    expect(weekHourRange([eventAt('a', '2026-10-07T06:30:00')]).startHour).toBe(6)
  })

  it('positions events and puts overlapping ones in separate lanes', () => {
    const placed = layoutDayEvents(
      [
        eventAt('a', '2026-10-07T18:00:00', '2026-10-07T20:00:00'),
        eventAt('b', '2026-10-07T19:00:00', '2026-10-07T21:00:00'),
        eventAt('c', '2026-10-07T21:00:00', '2026-10-07T22:00:00'),
      ],
      8,
    )
    const byId = Object.fromEntries(placed.map((item) => [item.event.id, item]))
    expect(byId.a).toMatchObject({ topMinutes: 600, heightMinutes: 120, column: 0, columns: 2 })
    expect(byId.b).toMatchObject({ column: 1, columns: 2 })
    expect(byId.c).toMatchObject({ column: 0, columns: 1 })
  })
})
