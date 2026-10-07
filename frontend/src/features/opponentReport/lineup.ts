import type { ActiveLineup, LineupPlayer, MapComparison, MapLifetime } from '../../services/opponentReportApi'
import { formatPercent } from './labels'
import { formatKd } from './individualForm'

/** Compact lifetime cell, mirroring the FACEIT match room ("113 m. · K/D 1.12"); "—" without data. */
export function lifetimeLabel(lifetime: MapLifetime | null | undefined): string {
  if (!lifetime || lifetime.matches === 0) return '—'
  return `${lifetime.matches} m. · K/D ${formatKd(lifetime.avgKdRatio)}`
}

/** Tooltip of a lifetime cell: players, win rate, share of their pool matches and whether it counts as "played a lot". */
export function lifetimeTitle(
  lifetime: MapLifetime | null | undefined,
  side: 'oni' | 'my',
): string | undefined {
  if (!lifetime || lifetime.matches === 0) return undefined
  const lines = [
    `${side === 'oni' ? 'Aktywny skład' : 'Nasi gracze'}: ${lifetime.players} graczy, ${lifetime.matches} meczów lifetime`,
    `WR ${formatPercent(lifetime.winRate)}, śr. K/D ${formatKd(lifetime.avgKdRatio)}, ${Math.round(lifetime.share)}% ich meczów w puli`,
  ]
  if (lifetime.experienced) lines.push('Grają ją indywidualnie dużo — najniższa waga w decyzjach')
  return lines.join('\n')
}

/** Short warning for our side of a row when the sample is too small to decide anything; null otherwise. */
export function ourSampleNote(map: MapComparison): string | null {
  if (!map.ourLowSample) return null
  return map.ourGames === 0 ? 'brak danych' : 'za mało danych'
}

/** Tooltip of our side: FACEIT/HarnasHub split and the smoothed win rate. */
export function ourTitle(map: MapComparison): string {
  const lines = [`FACEIT: ${map.ourFaceitGames}, HarnasHub: ${map.ourInternalGames}`]
  if (map.ourSmoothedWinRate != null) lines.push(`Wygładzony WR: ${formatPercent(map.ourSmoothedWinRate)}`)
  if (map.ourLowSample) lines.push('Za mało meczów (< 5), by bilans wpływał na rekomendację')
  return lines.join('\n')
}

/** "nick (8/10)" — a lineup player with team games in the lineup window. */
export function lineupPlayerLabel(player: LineupPlayer, windowGames: number): string {
  return windowGames > 0 ? `${player.nickname} (${player.recentTeamGames}/${windowGames})` : player.nickname
}

/** Tooltip of an inactive player: team games overall (official ones for a season lineup) and the last one. */
export function inactiveTitle(player: LineupPlayer, lineup?: Pick<ActiveLineup, 'source'>): string {
  const last = player.lastTeamGameAtUtc
    ? new Date(player.lastTeamGameAtUtc).toLocaleDateString('pl-PL')
    : 'brak'
  const label =
    lineup?.source === 'EseaSeason' ? 'Mecze oficjalne drużyny w oknie' : 'Mecze drużynowe w oknie'
  return `${label}: ${player.teamGames}, ostatni: ${last}`
}

/** Polish plural of "league match": 1 mecz ligowy, 2 mecze ligowe, 5 meczów ligowych. */
export function leagueMatchesLabel(count: number): string {
  const tens = count % 100
  if (count === 1) return '1 mecz ligowy'
  if (count % 10 >= 2 && count % 10 <= 4 && (tens < 12 || tens > 14)) return `${count} mecze ligowe`
  return `${count} meczów ligowych`
}

/** Lineup header: "Skład z sezonu ESEA S59 (4 mecze ligowe)" for a season lineup, otherwise "Aktywny skład". */
export function lineupHeading(lineup: ActiveLineup): string {
  if (lineup.source === 'EseaSeason' && lineup.season) {
    return `Skład z sezonu ESEA ${lineup.season} (${leagueMatchesLabel(lineup.windowGames)})`
  }
  return lineup.source === 'OfficialMatches' ? 'Skład z meczów oficjalnych drużyny' : 'Aktywny skład'
}

/** Summary of the collapsed list of team members left out of the lineup. */
export function inactiveSummary(lineup: ActiveLineup): string {
  return lineup.source === 'EseaSeason'
    ? `Pozostali członkowie drużyny FACEIT (nie grali w tym sezonie) (${lineup.inactive.length})`
    : `Byli / rezerwowi (${lineup.inactive.length}) — pominięci w statystykach graczy`
}

/** "ESEA 4 · razem 6" — their official vs ≥ 3-together games on a map; null without official team games or older reports. */
export function teamGamesSplitLabel(
  map: MapComparison,
  lineup: ActiveLineup | null | undefined,
): string | null {
  if (map.theirOfficialGames == null || map.theirGames === 0 || !lineup?.officialMatches) return null
  const official = lineup.source === 'EseaSeason' ? 'ESEA' : 'ofic.'
  return `${official} ${map.theirOfficialGames} · razem ${map.theirTogetherGames ?? 0}`
}
