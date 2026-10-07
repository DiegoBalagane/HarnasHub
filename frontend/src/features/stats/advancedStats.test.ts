import { describe, expect, it } from 'vitest'
import type { AdvancedFormPoint, AdvancedMapCell } from '../../services/advancedStatsApi'
import {
  advancedColumns,
  buildFormSeries,
  buildHeatmap,
  formatFraction,
  percent,
  ratingColor,
  sortPlayers,
} from './advancedStats'
import { makePlayer } from './advancedStats.fixtures'

const column = (key: string) => advancedColumns.find((c) => c.key === key)!

describe('advancedStats helpers', () => {
  it('computes percentages and fractions', () => {
    expect(percent(1, 4)).toBe(25)
    expect(percent(1, 0)).toBeNull()
    expect(formatFraction(4, 10)).toBe('4/10 (40%)')
    expect(formatFraction(0, 0)).toBe('—')
  })

  it('formats columns with Polish match-page wording', () => {
    const player = makePlayer()
    expect(column('tradeKills').label).toBe('Pomszczenia')
    expect(column('tradedDeaths').label).toBe('Zgony pomszczone')
    expect(column('tradedDeaths').format(player)).toBe('4/10 (40%)')
    expect(column('openingCt').format(player)).toBe('—')
    expect(column('bestClutch').format(player)).toBe('1v2')
    expect(column('rating').format(player)).toBe('1.12')
    expect(advancedColumns.every((c) => c.title.length > 0)).toBe(true)
  })

  it('sorts by value with missing values last in both directions', () => {
    const players = [
      makePlayer({ userId: 'a', name: 'A', avgRating: 1.0 }),
      makePlayer({ userId: 'b', name: 'B', avgRating: null }),
      makePlayer({ userId: 'c', name: 'C', avgRating: 1.3 }),
    ]
    expect(sortPlayers(players, column('rating'), true).map((p) => p.name)).toEqual(['C', 'A', 'B'])
    expect(sortPlayers(players, column('rating'), false).map((p) => p.name)).toEqual(['A', 'C', 'B'])
  })

  it('maps ratings to a red-to-green colour', () => {
    expect(ratingColor(0.5)).toContain('hsla(0,')
    expect(ratingColor(1.5)).toContain('hsla(120,')
  })

  it('builds a players x maps heatmap', () => {
    const cells: AdvancedMapCell[] = [
      { userId: 'u1', map: 'Mirage', matches: 2, avgRating: 1.1, avgAdr: 80 },
      { userId: 'u1', map: 'Dust2', matches: 1, avgRating: 0.9, avgAdr: 70 },
    ]
    const heatmap = buildHeatmap([makePlayer()], cells)
    expect(heatmap.maps).toEqual(['Dust2', 'Mirage'])
    expect(heatmap.rows[0].cells.Mirage?.matches).toBe(2)
  })

  it('places selected players on shared match slots in date order', () => {
    const point = (userId: string, matchResultId: string, playedAtUtc: string): AdvancedFormPoint => ({
      userId,
      matchResultId,
      playedAtUtc,
      opponent: 'X',
      map: 'Mirage',
      rating: 1,
      adr: 70,
    })
    const form = [
      point('u1', 'm2', '2026-02-01T00:00:00Z'),
      point('u1', 'm1', '2026-01-01T00:00:00Z'),
      point('u2', 'm2', '2026-02-01T00:00:00Z'),
      point('u3', 'm3', '2026-03-01T00:00:00Z'),
    ]
    const { matchIds, series } = buildFormSeries(form, ['u1', 'u2'])
    expect(matchIds).toEqual(['m1', 'm2'])
    expect(series[0].points.map((p) => p.index)).toEqual([0, 1])
    expect(series[1].points.map((p) => p.index)).toEqual([1])
  })
})
