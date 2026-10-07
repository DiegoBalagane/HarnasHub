import { describe, expect, it } from 'vitest'
import {
  addMinutesToLocalValue,
  addMinutesToTime,
  formatDateTimeValue,
  formatDateValue,
  isoUtcToLocalValue,
  joinLocalValue,
  localValueToIsoUtc,
  parseTimeText,
  snapTime,
  splitLocalValue,
  toLocalValue,
} from './dateTime'

describe('dateTime helpers', () => {
  it('round-trips a local value through UTC ISO', () => {
    const iso = localValueToIsoUtc('2026-10-07T20:30')
    expect(iso).toBe(new Date(2026, 9, 7, 20, 30).toISOString())
    expect(isoUtcToLocalValue(iso)).toBe('2026-10-07T20:30')
  })

  it('returns empty / null for missing or invalid input', () => {
    expect(isoUtcToLocalValue(null)).toBe('')
    expect(isoUtcToLocalValue('nope')).toBe('')
    expect(localValueToIsoUtc('')).toBeNull()
    expect(localValueToIsoUtc('2026-10-07')).toBeNull()
  })

  it('splits and joins date and time', () => {
    expect(splitLocalValue('2026-10-07T20:30')).toEqual({ date: '2026-10-07', time: '20:30' })
    expect(splitLocalValue('')).toEqual({ date: '', time: '' })
    expect(joinLocalValue('2026-10-07', '20:30')).toBe('2026-10-07T20:30')
    expect(joinLocalValue('2026-10-07', '')).toBe('')
  })

  it('parses typed times', () => {
    expect(parseTimeText('20:30')).toBe('20:30')
    expect(parseTimeText('2030')).toBe('20:30')
    expect(parseTimeText('830')).toBe('08:30')
    expect(parseTimeText('8')).toBe('08:00')
    expect(parseTimeText('24:00')).toBeNull()
    expect(parseTimeText('12:75')).toBeNull()
    expect(parseTimeText('abc')).toBeNull()
  })

  it('snaps time to the step without leaving the day', () => {
    expect(snapTime('20:32', 5)).toBe('20:30')
    expect(snapTime('20:33', 15)).toBe('20:30')
    expect(snapTime('23:59', 15)).toBe('23:59')
  })

  it('shifts datetimes and times across boundaries', () => {
    expect(addMinutesToLocalValue('2026-10-07T23:30', 60)).toBe('2026-10-08T00:30')
    expect(addMinutesToTime('23:30', 60)).toBe('00:30')
    expect(addMinutesToTime('00:15', -30)).toBe('23:45')
  })

  it('formats for the trigger in Polish', () => {
    expect(formatDateTimeValue('2026-10-07T20:30')).toBe('śr, 7 paź 2026 · 20:30')
    expect(formatDateValue('2026-10-07')).toBe('śr, 7 paź 2026')
    expect(formatDateTimeValue('')).toBe('')
  })

  it('formats a Date as a local value', () => {
    expect(toLocalValue(new Date(2026, 0, 5, 7, 4))).toBe('2026-01-05T07:04')
  })
})
