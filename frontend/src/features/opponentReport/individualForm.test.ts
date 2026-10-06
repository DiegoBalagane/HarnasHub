import { describe, expect, it } from 'vitest'
import type { MapComfort } from '../../services/opponentReportApi'
import { comfortLabel, comfortTitle, formatDelta, formatKd, formTooltip, topMaps } from './individualForm'
import { makeMap, makePlayer } from './individualForm.fixtures'

const comfort: MapComfort = {
  mapName: 'Ancient',
  ratedPlayers: 5,
  regularPlayers: 1,
  avoidingPlayers: 4,
  soloShare: 3,
  avgWinRate: 52.4,
  avgKdRatio: 1.1,
  regularNicknames: ['a'],
  avoidingNicknames: ['b', 'c', 'd', 'e'],
}

describe('individual form helpers', () => {
  it('formats K/D and signed deltas', () => {
    expect(formatKd(null)).toBe('—')
    expect(formatKd(1.4)).toBe('1.40')
    expect(formatDelta(0.4, 2)).toBe('+0.40')
    expect(formatDelta(-12)).toBe('-12')
    expect(formatDelta(0)).toBe('0')
  })

  it('describes recent form in the tooltip', () => {
    const text = formTooltip({
      recentGames: 10,
      earlierGames: 15,
      recentKdRatio: 1.45,
      earlierKdRatio: 1.05,
      recentWinRate: 60,
      earlierWinRate: 48,
      kdDelta: 0.4,
      winRateDelta: 12,
      direction: 'Up',
    })
    expect(text).toContain('K/D 1.45 (wcześniej 1.05, +0.40)')
    expect(text).toContain('WR 60% (wcześniej 48%, +12 pp)')
  })

  it('picks the most played maps for chips', () => {
    const player = makePlayer({ maps: [makeMap('Nuke', 2), makeMap('Mirage', 9), makeMap('Inferno', 5), makeMap('Ancient', 1)] })
    expect(topMaps(player).map((m) => m.mapName)).toEqual(['Mirage', 'Inferno', 'Nuke'])
  })

  it('summarises comfort compactly and in detail', () => {
    expect(comfortLabel(comfort)).toBe('1/5 grają solo · 4 unikają')
    expect(comfortLabel({ ...comfort, avoidingPlayers: 1 })).toBe('1/5 grają solo · 1 unika')
    expect(comfortLabel({ ...comfort, avoidingPlayers: 0 })).toBe('1/5 grają solo')
    expect(comfortLabel({ ...comfort, ratedPlayers: 0 })).toBeNull()
    expect(comfortLabel(undefined)).toBeNull()
    expect(comfortTitle(comfort)).toContain('Unikają (0–1 mecz): b, c, d, e')
    expect(comfortTitle(comfort)).toContain('Śr. WR solo: 52%, K/D 1.10')
  })
})
