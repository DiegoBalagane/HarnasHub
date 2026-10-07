import type { TeamTrendPoint } from '../../services/statsApi'

/** "1W / 1R / 1L" record of a trend point; the draws part only shows when there were any. */
export function recordLabel(point: Pick<TeamTrendPoint, 'cumulativeWins' | 'cumulativeLosses' | 'cumulativeDraws'>): string {
  const draws = point.cumulativeDraws ?? 0
  return draws > 0
    ? `${point.cumulativeWins}W / ${draws}R / ${point.cumulativeLosses}L`
    : `${point.cumulativeWins}W / ${point.cumulativeLosses}L`
}
