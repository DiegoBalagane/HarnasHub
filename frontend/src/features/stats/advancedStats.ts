import type { AdvancedFormPoint, AdvancedMapCell, AdvancedPlayer } from '../../services/advancedStatsApi'

/** Placeholder shown when a value is unavailable. */
export const EMPTY_VALUE = '—'

/** Options of the "last N matches" filter; null means every analysed match. */
export const lastOptions: { value: number | null; label: string }[] = [
  { value: 5, label: 'Ostatnie 5' },
  { value: 10, label: 'Ostatnie 10' },
  { value: 20, label: 'Ostatnie 20' },
  { value: null, label: 'Wszystkie' },
]

/** Percentage (0-100) of part in total, or null when total is 0. */
export function percent(part: number, total: number): number | null {
  return total > 0 ? (part / total) * 100 : null
}

/** "x/y (z%)" or the placeholder when y is 0. */
export function formatFraction(part: number, total: number): string {
  const value = percent(part, total)
  return value === null ? EMPTY_VALUE : `${part}/${total} (${value.toFixed(0)}%)`
}

/** Fixed-digit number or the placeholder for null. */
export function formatNumber(value: number | null, digits = 1): string {
  return value === null ? EMPTY_VALUE : value.toFixed(digits)
}

/** One column of the per-player table: sort value, display text and a Polish tooltip. */
export interface AdvancedColumn {
  key: string
  label: string
  title: string
  value: (player: AdvancedPlayer) => number | null
  format: (player: AdvancedPlayer) => string
}

/** Columns of the advanced table, in display order. */
export const advancedColumns: AdvancedColumn[] = [
  {
    key: 'matches',
    label: 'Mecze',
    title: 'Liczba meczów z demką, w których gracz wystąpił',
    value: (p) => p.matches,
    format: (p) => String(p.matches),
  },
  {
    key: 'rounds',
    label: 'Rundy',
    title: 'Liczba rozegranych rund',
    value: (p) => p.rounds,
    format: (p) => String(p.rounds),
  },
  {
    key: 'openingT',
    label: 'Otwarcia T',
    title: 'Wygrane pojedynki otwierające rundę (pierwsze zabójstwo) po stronie T — wygrane/wszystkie i procent',
    value: (p) => percent(p.openingWonT, p.openingWonT + p.openingLostT),
    format: (p) => formatFraction(p.openingWonT, p.openingWonT + p.openingLostT),
  },
  {
    key: 'openingCt',
    label: 'Otwarcia CT',
    title: 'Wygrane pojedynki otwierające rundę (pierwsze zabójstwo) po stronie CT — wygrane/wszystkie i procent',
    value: (p) => percent(p.openingWonCt, p.openingWonCt + p.openingLostCt),
    format: (p) => formatFraction(p.openingWonCt, p.openingWonCt + p.openingLostCt),
  },
  {
    key: 'tradeKills',
    label: 'Pomszczenia',
    title: 'Zabójstwa, którymi gracz pomścił śmierć kolegi w ciągu 5 sekund',
    value: (p) => p.tradeKills,
    format: (p) => String(p.tradeKills),
  },
  {
    key: 'tradedDeaths',
    label: 'Zgony pomszczone',
    title: 'Śmierci gracza, które kolega pomścił w ciągu 5 sekund — pomszczone/wszystkie śmierci i procent',
    value: (p) => percent(p.tradedDeaths, p.deaths),
    format: (p) => formatFraction(p.tradedDeaths, p.deaths),
  },
  {
    key: 'clutches',
    label: 'Clutche',
    title: 'Wygrane sytuacje 1vX — wygrane/podjęte i procent',
    value: (p) => percent(p.clutchesWon, p.clutchAttempts),
    format: (p) => formatFraction(p.clutchesWon, p.clutchAttempts),
  },
  {
    key: 'bestClutch',
    label: 'Najlepszy clutch',
    title: 'Największy wygrany clutch (1vX)',
    value: (p) => (p.bestClutchWon > 0 ? p.bestClutchWon : null),
    format: (p) => (p.bestClutchWon > 0 ? `1v${p.bestClutchWon}` : EMPTY_VALUE),
  },
  {
    key: 'enemiesFlashed',
    label: 'Oślepieni wrogowie',
    title: 'Liczba wrogów oślepionych na co najmniej 0,5 s',
    value: (p) => p.enemiesFlashed,
    format: (p) => String(p.enemiesFlashed),
  },
  {
    key: 'avgBlind',
    label: 'Śr. oślepienie (s)',
    title: 'Średni czas oślepienia wroga w sekundach',
    value: (p) => (p.enemiesFlashed > 0 ? p.avgBlindSeconds : null),
    format: (p) => (p.enemiesFlashed > 0 ? p.avgBlindSeconds.toFixed(2) : EMPTY_VALUE),
  },
  {
    key: 'teamFlashes',
    label: 'Oślepieni swoi',
    title: 'Liczba oślepień kolegów z drużyny (mniej = lepiej)',
    value: (p) => p.teamFlashes,
    format: (p) => String(p.teamFlashes),
  },
  {
    key: 'utility',
    label: 'Obr. granatami/rundę',
    title: 'Średnie obrażenia zadane granatami i ogniem na rundę',
    value: (p) => p.utilityDamagePerRound,
    format: (p) => formatNumber(p.utilityDamagePerRound),
  },
  {
    key: 'kast',
    label: 'KAST%',
    title: 'Średni odsetek rund z zabójstwem, asystą, przeżyciem lub pomszczeniem (łącznie, bez podziału na strony)',
    value: (p) => p.avgKast,
    format: (p) => (p.avgKast === null ? EMPTY_VALUE : `${p.avgKast.toFixed(0)}%`),
  },
  {
    key: 'adr',
    label: 'ADR',
    title: 'Średnie obrażenia na rundę',
    value: (p) => p.avgAdr,
    format: (p) => formatNumber(p.avgAdr),
  },
  {
    key: 'rating',
    label: 'Rating',
    title: 'Średni rating (łącznie, bez podziału na strony)',
    value: (p) => p.avgRating,
    format: (p) => formatNumber(p.avgRating, 2),
  },
]

/** Players sorted by the column's value (missing values always last); ties keep the name order. */
export function sortPlayers(players: AdvancedPlayer[], column: AdvancedColumn, desc: boolean): AdvancedPlayer[] {
  return [...players].sort((a, b) => {
    const left = column.value(a)
    const right = column.value(b)
    if (left === null && right === null) return a.name.localeCompare(b.name, 'pl')
    if (left === null) return 1
    if (right === null) return -1
    return (left - right) * (desc ? -1 : 1) || a.name.localeCompare(b.name, 'pl')
  })
}

/** Heatmap background for a rating: red at 0.7 and below, neutral-ish amber at 1.0, green at 1.3 and above. */
export function ratingColor(rating: number): string {
  const ratio = Math.min(1, Math.max(0, (rating - 0.7) / 0.6))
  return `hsla(${Math.round(ratio * 120)}, 55%, 32%, 0.85)`
}

/** Heatmap model: the maps present (alphabetical) and per player a map → cell lookup. */
export interface Heatmap {
  maps: string[]
  rows: { player: AdvancedPlayer; cells: Record<string, AdvancedMapCell | undefined> }[]
}

/** Builds the players × maps grid from the flat cell list. */
export function buildHeatmap(players: AdvancedPlayer[], cells: AdvancedMapCell[]): Heatmap {
  const maps = [...new Set(cells.map((cell) => cell.map))].sort((a, b) => a.localeCompare(b, 'pl'))
  const rows = players.map((player) => ({
    player,
    cells: Object.fromEntries(cells.filter((cell) => cell.userId === player.userId).map((cell) => [cell.map, cell])),
  }))
  return { maps, rows }
}

/** One plotted series of the form chart. */
export interface FormSeries {
  userId: string
  points: { index: number; point: AdvancedFormPoint }[]
}

/** Form chart model: matches in date order (x slots) and one series per selected player placed on those slots. */
export function buildFormSeries(
  form: AdvancedFormPoint[],
  selected: string[],
): { matchIds: string[]; series: FormSeries[] } {
  const chosen = form.filter((point) => selected.includes(point.userId))
  const dates = new Map<string, string>()
  for (const point of chosen) dates.set(point.matchResultId, point.playedAtUtc)
  const matchIds = [...dates.keys()].sort((a, b) => dates.get(a)!.localeCompare(dates.get(b)!))
  const series = selected.map((userId) => ({
    userId,
    points: chosen
      .filter((point) => point.userId === userId)
      .map((point) => ({ index: matchIds.indexOf(point.matchResultId), point }))
      .sort((a, b) => a.index - b.index),
  }))
  return { matchIds, series }
}
