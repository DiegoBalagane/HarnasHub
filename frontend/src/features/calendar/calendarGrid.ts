import type { CalendarEvent } from '../../services/calendarApi'
import { addDaysIso, parseIsoDate, toIsoDate } from '../availability/weekDates'

/** Calendar views the page can switch between. */
export type CalendarView = 'month' | 'week' | 'agenda'

const daysInWeek = 7
const minutesPerHour = 60
const minutesPerDay = 24 * minutesPerHour
/** Duration assumed for an event without an end time. */
const defaultDurationMinutes = 60
/** Minimum height of any block, so short events stay clickable. */
const minBlockMinutes = 30
const defaultStartHour = 8
const defaultEndHour = 24

/** Returns the yyyy-MM-dd of the Monday on or before the given date. */
export function startOfWeekIso(iso: string): string {
  const dayOfWeek = parseIsoDate(iso).getDay()
  return addDaysIso(iso, -((dayOfWeek + 6) % daysInWeek))
}

/** Returns the seven consecutive days (Monday first) of the week containing the date. */
export function weekDaysOf(iso: string): string[] {
  const start = startOfWeekIso(iso)
  return Array.from({ length: daysInWeek }, (_, offset) => addDaysIso(start, offset))
}

/** First day of the month containing the date, as yyyy-MM-dd. */
export function startOfMonthIso(iso: string): string {
  return `${iso.slice(0, 7)}-01`
}

/** Shifts a date by whole months, landing on day 1 so month navigation never skips a month. */
export function addMonthsIso(iso: string, months: number): string {
  const date = parseIsoDate(startOfMonthIso(iso))
  date.setMonth(date.getMonth() + months)
  return toIsoDate(date)
}

/** Month grid as full Monday-first weeks (5 or 6 rows) covering every day of the month, padded with neighbouring days. */
export function buildMonthMatrix(anchorIso: string): string[][] {
  const first = startOfMonthIso(anchorIso)
  const monthKey = first.slice(0, 7)
  const weeks: string[][] = []
  let cursor = startOfWeekIso(first)

  do {
    weeks.push(Array.from({ length: daysInWeek }, (_, offset) => addDaysIso(cursor, offset)))
    cursor = addDaysIso(cursor, daysInWeek)
  } while (cursor.slice(0, 7) <= monthKey)

  return weeks
}

/** Moves the anchor one step in the given view: a week for Week, a month otherwise. */
export function shiftAnchor(anchorIso: string, view: CalendarView, direction: 1 | -1): string {
  return view === 'week' ? addDaysIso(anchorIso, direction * daysInWeek) : addMonthsIso(anchorIso, direction)
}

/** Groups events by their local start day, each day sorted by start time. */
export function groupEventsByDate(events: readonly CalendarEvent[]): Map<string, CalendarEvent[]> {
  const grouped = new Map<string, CalendarEvent[]>()

  for (const event of events) {
    const key = toIsoDate(new Date(event.startsAtUtc))
    grouped.set(key, [...(grouped.get(key) ?? []), event])
  }

  for (const list of grouped.values()) {
    list.sort((left, right) => left.startsAtUtc.localeCompare(right.startsAtUtc))
  }

  return grouped
}

function startMinutes(event: CalendarEvent): number {
  const start = new Date(event.startsAtUtc)
  return start.getHours() * minutesPerHour + start.getMinutes()
}

/** End in minutes since midnight of the start day, clamped to that day's end. */
function endMinutes(event: CalendarEvent): number {
  const start = startMinutes(event)
  const raw = event.endsAtUtc
    ? start + (new Date(event.endsAtUtc).getTime() - new Date(event.startsAtUtc).getTime()) / 60_000
    : start + defaultDurationMinutes

  return Math.min(minutesPerDay, Math.max(start + minBlockMinutes, raw))
}

/** Visible hour range [startHour, endHour) of a week grid: 8-24 by default, widened to fit earlier events. */
export function weekHourRange(events: readonly CalendarEvent[]): { startHour: number; endHour: number } {
  let startHour = defaultStartHour
  let endHour = defaultEndHour

  for (const event of events) {
    startHour = Math.min(startHour, Math.floor(startMinutes(event) / minutesPerHour))
    endHour = Math.max(endHour, Math.ceil(endMinutes(event) / minutesPerHour))
  }

  return { startHour, endHour: Math.min(24, endHour) }
}

/** One event placed in a day column: vertical position in minutes from the grid top, and its horizontal lane. */
export interface PositionedEvent {
  event: CalendarEvent
  topMinutes: number
  heightMinutes: number
  column: number
  columns: number
}

interface LaidOut {
  event: CalendarEvent
  start: number
  end: number
  column: number
}

/** Lays out one day's events as blocks; overlapping events share the width in side-by-side lanes. */
export function layoutDayEvents(events: readonly CalendarEvent[], startHour: number): PositionedEvent[] {
  const items = events
    .map((event) => ({ event, start: startMinutes(event), end: endMinutes(event) }))
    .sort((left, right) => left.start - right.start || right.end - left.end)

  const result: PositionedEvent[] = []
  let cluster: LaidOut[] = []
  let laneEnds: number[] = []
  let clusterEnd = -1

  function flush() {
    const columns = laneEnds.length
    for (const item of cluster) {
      result.push({
        event: item.event,
        topMinutes: item.start - startHour * minutesPerHour,
        heightMinutes: item.end - item.start,
        column: item.column,
        columns,
      })
    }
    cluster = []
    laneEnds = []
  }

  for (const item of items) {
    if (cluster.length > 0 && item.start >= clusterEnd) {
      flush()
    }

    let column = laneEnds.findIndex((laneEnd) => laneEnd <= item.start)
    if (column === -1) {
      column = laneEnds.length
    }
    laneEnds[column] = item.end
    clusterEnd = cluster.length === 0 ? item.end : Math.max(clusterEnd, item.end)
    cluster.push({ ...item, column })
  }
  flush()

  return result
}

/** UTC ISO timestamp for a local day + hour — prefills the create form from a clicked slot. */
export function toStartsAtUtc(dateIso: string, hour: number): string {
  const date = parseIsoDate(dateIso)
  date.setHours(hour, 0, 0, 0)
  return date.toISOString()
}

const monthFormatter = new Intl.DateTimeFormat('pl-PL', { month: 'long', year: 'numeric' })
const dayMonthFormatter = new Intl.DateTimeFormat('pl-PL', { day: 'numeric', month: 'long' })
const dayMonthShortFormatter = new Intl.DateTimeFormat('pl-PL', { day: 'numeric', month: 'short' })

/** Polish range label: "październik 2026" for month/agenda, "5 – 11 października 2026" for a week. */
export function formatRangeLabel(anchorIso: string, view: CalendarView): string {
  if (view !== 'week') {
    return monthFormatter.format(parseIsoDate(anchorIso))
  }

  const days = weekDaysOf(anchorIso)
  const first = parseIsoDate(days[0])
  const last = parseIsoDate(days[daysInWeek - 1])
  const startLabel =
    first.getMonth() === last.getMonth() ? String(first.getDate()) : dayMonthShortFormatter.format(first)

  return `${startLabel} – ${dayMonthFormatter.format(last)} ${last.getFullYear()}`
}
