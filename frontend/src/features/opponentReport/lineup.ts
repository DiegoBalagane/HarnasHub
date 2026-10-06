import type { LineupPlayer, MapComparison, MapLifetime } from '../../services/opponentReportApi'
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

/** Tooltip of an inactive player: team games overall and the last one. */
export function inactiveTitle(player: LineupPlayer): string {
  const last = player.lastTeamGameAtUtc
    ? new Date(player.lastTeamGameAtUtc).toLocaleDateString('pl-PL')
    : 'brak'
  return `Mecze drużynowe w oknie: ${player.teamGames}, ostatni: ${last}`
}
