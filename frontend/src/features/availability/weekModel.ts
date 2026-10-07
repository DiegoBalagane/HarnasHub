import type { DayStatusView, MemberWeek } from '../../services/availabilityApi'
import { dayStatusLabels, vacationIcon, vacationLabel } from './labels'
import type { DaySummary } from './daySummary'
import { entryFor, toShortTime } from './weekDates'

export type PillKind = 'available' | 'partial' | 'vacation' | 'off' | 'none'

export interface PillView {
  kind: PillKind
  /** Short text rendered inside the pill. */
  text: string
  /** Full status name used for tooltips and accessible names. */
  label: string
}

/** "18:00" -> "18", "18:30" -> "18:30": compact hour form used in pills and headers. */
export function shortHour(time: string): string {
  const short = time.slice(0, 5)
  return short.endsWith(':00') ? short.slice(0, 2).replace(/^0/, '') : short
}

/** Maps a day entry (or a missing one) to the kind/text shown in a pill. */
export function pillView(entry: DayStatusView | undefined): PillView {
  if (!entry) return { kind: 'none', text: '—', label: dayStatusLabels.NotSet }
  if (entry.isVacation) return { kind: 'vacation', text: `${vacationIcon} ${vacationLabel}`, label: vacationLabel }

  switch (entry.status) {
    case 'Available':
      return { kind: 'available', text: 'Dostępny', label: dayStatusLabels.Available }
    case 'PartiallyAvailable': {
      const from = toShortTime(entry.from)
      const to = toShortTime(entry.to)
      const text = from !== null && to !== null ? `${shortHour(from)}–${shortHour(to)}` : 'Częściowo'
      return { kind: 'partial', text, label: dayStatusLabels.PartiallyAvailable }
    }
    case 'Off':
      return { kind: 'off', text: 'Off', label: dayStatusLabels.Off }
    default:
      return { kind: 'none', text: '—', label: dayStatusLabels.NotSet }
  }
}

export type FillTone = 'success' | 'warning' | 'danger' | 'none'

/** Colour tone of the main-roster fill bar: full = success, at least 60% = warning, otherwise danger. */
export function fillTone(available: number, total: number): FillTone {
  if (total <= 0) return 'none'
  if (available >= total) return 'success'
  return available / total >= 0.6 ? 'warning' : 'danger'
}

/** Plain-text tooltip for a day header: bench counts, the common window and who is not playing. */
export function describeDay(summary: DaySummary, absentNames: string[], eventTitles: string[]): string {
  const lines = [
    `Main: ${summary.main.availableCount}/${summary.main.totalCount}`,
    `Reszta: ${summary.rest.availableCount}/${summary.rest.totalCount}`,
  ]
  if (summary.commonWindow) lines.push(`Wspólne okno: ${summary.commonWindow.from}–${summary.commonWindow.to}`)
  if (absentNames.length > 0) lines.push(`Nie gra: ${absentNames.join(', ')}`)
  for (const title of eventTitles) lines.push(`Wydarzenie: ${title}`)
  return lines.join('\n')
}

/** One-phrase summary of a member's whole week, e.g. "off cały tydzień". */
export function summarizeWeek(member: MemberWeek, dates: string[]): string {
  const kinds = dates.map((date) => pillView(entryFor(member, date)).kind)
  const first = kinds[0]

  if (kinds.every((kind) => kind === first)) {
    const phrases: Record<PillKind, string> = {
      available: 'dostępny cały tydzień',
      partial: 'częściowo dostępny cały tydzień',
      vacation: 'urlop cały tydzień',
      off: 'off cały tydzień',
      none: 'brak odpowiedzi',
    }
    return phrases[first]
  }

  const playing = kinds.filter((kind) => kind === 'available' || kind === 'partial').length
  return `dostępny ${playing} z ${dates.length} dni`
}

/** Column template shared by the grid header, player rows and the coach footer. */
export function gridColumns(dayCount: number): string {
  return `minmax(130px,170px) repeat(${dayCount}, minmax(0,1fr))`
}
