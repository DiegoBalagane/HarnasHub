import type { FormDirection, MapComfort, PlayerForm, PlayerMapForm, PlayerRecentForm } from '../../services/opponentReportApi'

/** How many map chips a player card shows. */
export const TOP_MAP_CHIPS = 3

/** Arrow, colour and Polish label of a form direction (semantic success/danger tokens). */
export const formArrows: Record<FormDirection, { symbol: string; className: string; label: string }> = {
  Up: { symbol: '↑', className: 'text-success-400', label: 'forma w górę' },
  Down: { symbol: '↓', className: 'text-danger-400', label: 'forma w dół' },
  Flat: { symbol: '→', className: 'text-neutral-400', label: 'forma stabilna' },
}

/** A K/D ratio with two decimals, "—" when unknown. */
export function formatKd(value: number | null): string {
  return value === null ? '—' : value.toFixed(2)
}

/** A signed number with the given decimals ("+0.40", "-12"). */
export function formatDelta(value: number, decimals = 0): string {
  const text = value.toFixed(decimals)
  return value > 0 ? `+${text}` : text
}

/** Tooltip of the form arrow: last games vs earlier, K/D and win rate. */
export function formTooltip(form: PlayerRecentForm): string {
  return [
    `Ostatnie ${form.recentGames} meczów vs wcześniejsze ${form.earlierGames}`,
    `K/D ${formatKd(form.recentKdRatio)} (wcześniej ${formatKd(form.earlierKdRatio)}, ${formatDelta(form.kdDelta, 2)})`,
    `WR ${Math.round(form.recentWinRate)}% (wcześniej ${Math.round(form.earlierWinRate)}%, ${formatDelta(form.winRateDelta)} pp)`,
  ].join('\n')
}

/** The player's most played pool maps for the chips. */
export function topMaps(player: PlayerForm, count = TOP_MAP_CHIPS): PlayerMapForm[] {
  return [...player.maps].sort((a, b) => b.games - a.games).slice(0, count)
}

/** Compact comfort text for the map matrix ("4/5 grają solo · 2 unikają"); null without rated players. */
export function comfortLabel(comfort: MapComfort | undefined): string | null {
  if (!comfort || comfort.ratedPlayers === 0) return null
  const parts = [`${comfort.regularPlayers}/${comfort.ratedPlayers} grają solo`]
  if (comfort.avoidingPlayers > 0) parts.push(`${comfort.avoidingPlayers} ${comfort.avoidingPlayers === 1 ? 'unika' : 'unikają'}`)
  return parts.join(' · ')
}

/** Tooltip of the comfort cell: who plays the map regularly, who avoids it, and the regulars' averages. */
export function comfortTitle(comfort: MapComfort): string {
  const lines = [`Ocenieni gracze (≥ 10 meczów solo): ${comfort.ratedPlayers}`]
  if (comfort.regularNicknames.length > 0) lines.push(`Regularnie (≥ 3 mecze): ${comfort.regularNicknames.join(', ')}`)
  if (comfort.avoidingNicknames.length > 0) lines.push(`Unikają (0–1 mecz): ${comfort.avoidingNicknames.join(', ')}`)
  if (comfort.avgWinRate !== null) lines.push(`Śr. WR solo: ${Math.round(comfort.avgWinRate)}%, K/D ${formatKd(comfort.avgKdRatio)}`)
  return lines.join('\n')
}

/** Team/solo split of a map row ("3 / 12"), with the per-split K/D in the tooltip. */
export function splitTitle(map: PlayerMapForm): string {
  return [
    `Drużynowo: ${map.teamGames} m., WR ${map.teamWinRate === null ? '—' : `${Math.round(map.teamWinRate)}%`}, K/D ${formatKd(map.teamKdRatio)}`,
    `Solo: ${map.soloGames} m., WR ${map.soloWinRate === null ? '—' : `${Math.round(map.soloWinRate)}%`}, K/D ${formatKd(map.soloKdRatio)}`,
  ].join('\n')
}
