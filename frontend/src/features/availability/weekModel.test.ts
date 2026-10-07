import { describe, expect, it } from 'vitest'
import { chipSelection, eligibleBulkDays } from './bulkDays'
import { makeMember, testToday } from './availabilityTestData'
import { buildWeekDates } from './weekDates'
import { fillTone, pillView, shortHour, summarizeWeek } from './weekModel'

const dates = buildWeekDates(testToday)

describe('weekModel', () => {
  it('shortens full hours and keeps minutes', () => {
    expect(shortHour('18:00:00')).toBe('18')
    expect(shortHour('09:30')).toBe('09:30')
  })

  it('maps entries to pill kinds', () => {
    const base = { from: null, to: null, isVacation: false }
    expect(pillView({ ...base, status: 'Available' }).kind).toBe('available')
    expect(pillView({ ...base, status: 'PartiallyAvailable', from: '16:00:00', to: '21:00:00' }).text).toBe('16–21')
    expect(pillView({ ...base, status: 'Off', isVacation: true }).kind).toBe('vacation')
    expect(pillView({ ...base, status: 'Off' }).kind).toBe('off')
    expect(pillView({ ...base, status: 'NotSet' }).kind).toBe('none')
    expect(pillView(undefined).kind).toBe('none')
  })

  it('picks the fill tone from the ratio', () => {
    expect(fillTone(5, 5)).toBe('success')
    expect(fillTone(3, 5)).toBe('warning')
    expect(fillTone(1, 5)).toBe('danger')
    expect(fillTone(0, 0)).toBe('none')
  })

  it('summarises a whole week', () => {
    expect(summarizeWeek(makeMember('c', 'Capybar', { base: { status: 'Off' } }), dates)).toBe('off cały tydzień')
    const mixed = makeMember('c', 'Capybar', { overrides: { [testToday]: { status: 'Available' } } })
    expect(summarizeWeek(mixed, dates)).toBe('dostępny 1 z 7 dni')
  })
})

describe('bulkDays', () => {
  it('excludes past days and vacation days from the eligible set', () => {
    const week = buildWeekDates('2026-10-05')
    const member = makeMember('me', 'Ja', { start: '2026-10-05', overrides: { '2026-10-09': { isVacation: true, status: 'Off' } } })
    const eligible = eligibleBulkDays(member, week, testToday)

    expect(eligible).not.toContain('2026-10-05')
    expect(eligible).not.toContain('2026-10-06')
    expect(eligible).not.toContain('2026-10-09')
    expect(eligible).toContain('2026-10-07')
  })

  it('splits chips into workdays and weekend', () => {
    // 2026-10-07 is a Wednesday, so Sat/Sun are the 10th and 11th.
    expect(chipSelection('workdays', dates)).toEqual(['2026-10-07', '2026-10-08', '2026-10-09', '2026-10-12', '2026-10-13'])
    expect(chipSelection('weekend', dates)).toEqual(['2026-10-10', '2026-10-11'])
    expect(chipSelection('all', dates)).toEqual(dates)
  })
})
