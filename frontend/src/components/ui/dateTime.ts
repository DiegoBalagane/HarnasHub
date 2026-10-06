import { parseIsoDate, toIsoDate } from '../../features/availability/weekDates'

/** Local wall-clock datetime as the pickers hold it: `yyyy-MM-ddTHH:mm` (no zone); '' means "not set". */
export type LocalDateTime = string

const pad = (value: number) => String(value).padStart(2, '0')
const minutesPerDay = 24 * 60

/** Formats a Date as local `yyyy-MM-ddTHH:mm`. */
export function toLocalValue(date: Date): LocalDateTime {
  return `${toIsoDate(date)}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

/** A UTC ISO timestamp as the local `yyyy-MM-ddTHH:mm` value; '' when missing or invalid. */
export function isoUtcToLocalValue(iso: string | null | undefined): LocalDateTime {
  if (!iso) return ''
  const date = new Date(iso)
  return Number.isNaN(date.getTime()) ? '' : toLocalValue(date)
}

/** A local `yyyy-MM-ddTHH:mm` value as a UTC ISO timestamp (what the API expects); null when empty or invalid. */
export function localValueToIsoUtc(value: LocalDateTime): string | null {
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/.test(value)) return null
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? null : date.toISOString()
}

/** Splits `yyyy-MM-ddTHH:mm` into its date (`yyyy-MM-dd`) and time (`HH:mm`) parts; either is '' when absent. */
export function splitLocalValue(value: LocalDateTime): { date: string; time: string } {
  const [date = '', time = ''] = value.split('T')
  return { date, time }
}

/** Joins a date and a time into `yyyy-MM-ddTHH:mm`; '' unless both are present. */
export function joinLocalValue(date: string, time: string): LocalDateTime {
  return date && time ? `${date}T${time}` : ''
}

/** Parses typed time ("20:30", "2030", "830", "8", "8:5") into `HH:mm`; null when it isn't a valid time of day. */
export function parseTimeText(text: string): string | null {
  const trimmed = text.trim().replace('.', ':')
  let hours: number
  let minutes: number
  const colon = /^(\d{1,2}):(\d{1,2})$/.exec(trimmed)
  if (colon) {
    hours = Number(colon[1])
    minutes = Number(colon[2])
  } else if (/^\d{3,4}$/.test(trimmed)) {
    hours = Number(trimmed.slice(0, -2))
    minutes = Number(trimmed.slice(-2))
  } else if (/^\d{1,2}$/.test(trimmed)) {
    hours = Number(trimmed)
    minutes = 0
  } else {
    return null
  }
  if (hours > 23 || minutes > 59) return null
  return `${pad(hours)}:${pad(minutes)}`
}

/** Rounds `HH:mm` to the nearest multiple of `step` minutes, clamped to 23:59 at the end of the day. */
export function snapTime(time: string, step: number): string {
  const parsed = parseTimeText(time)
  if (!parsed) return time
  const [hours, minutes] = parsed.split(':').map(Number)
  const snapped = Math.min(Math.round((hours * 60 + minutes) / step) * step, minutesPerDay - 1)
  return `${pad(Math.floor(snapped / 60))}:${pad(snapped % 60)}`
}

/** Shifts a local datetime by minutes (may cross days); '' stays ''. */
export function addMinutesToLocalValue(value: LocalDateTime, minutes: number): LocalDateTime {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  date.setMinutes(date.getMinutes() + minutes)
  return toLocalValue(date)
}

/** Shifts a time of day by minutes, wrapping around midnight. */
export function addMinutesToTime(time: string, minutes: number): string {
  const parsed = parseTimeText(time) ?? '00:00'
  const [hours, mins] = parsed.split(':').map(Number)
  const total = (((hours * 60 + mins + minutes) % minutesPerDay) + minutesPerDay) % minutesPerDay
  return `${pad(Math.floor(total / 60))}:${pad(total % 60)}`
}

const dateParts = new Intl.DateTimeFormat('pl-PL', { weekday: 'short', day: 'numeric', month: 'short', year: 'numeric' })

/** `yyyy-MM-dd` as "wt, 7 paź 2026"; '' for an empty/invalid value. */
export function formatDateValue(iso: string): string {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(iso)) return ''
  const parts = Object.fromEntries(dateParts.formatToParts(parseIsoDate(iso)).map((part) => [part.type, part.value.replace(/\.$/, '')]))
  return `${parts.weekday}, ${parts.day} ${parts.month} ${parts.year}`
}

/** `yyyy-MM-ddTHH:mm` as "wt, 7 paź 2026 · 20:30"; '' for an empty/invalid value. */
export function formatDateTimeValue(value: LocalDateTime): string {
  const { date, time } = splitLocalValue(value)
  const formattedDate = formatDateValue(date)
  return formattedDate && time ? `${formattedDate} · ${time}` : ''
}

/** Today as `yyyy-MM-dd` in local time. */
export function todayIso(now: Date = new Date()): string {
  return toIsoDate(now)
}
