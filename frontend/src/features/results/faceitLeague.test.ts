import { describe, expect, it } from 'vitest'
import { eseaSeasonNumber, matchLeagueForCompetition } from './faceitPrefill'

const leagues = [
  { id: 'l58', name: 'ESEA', season: 'Season 58 (Online)' },
  { id: 'l59', name: 'ESEA', season: 'Season 59 (Online)' },
  { id: 'x', name: 'Liga lokalna', season: '2026' },
]

describe('ESEA league prefill', () => {
  it('reads the season number from FACEIT and league names', () => {
    expect(eseaSeasonNumber('S59 EU Open10 D - Regular Season')).toBe(59)
    expect(eseaSeasonNumber('ESEA — Season 59 (Online)')).toBe(59)
    expect(eseaSeasonNumber('Europe 5v5 Queue')).toBeNull()
  })

  it('picks the single league of the same season', () => {
    expect(matchLeagueForCompetition(leagues, 'S59 EU Open10 D - Regular Season')?.id).toBe('l59')
    expect(matchLeagueForCompetition(leagues, 'S60 EU Open10 D')).toBeNull()
    expect(matchLeagueForCompetition(leagues, null)).toBeNull()
  })
})
