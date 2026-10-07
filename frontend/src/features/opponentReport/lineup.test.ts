import { describe, expect, it } from 'vitest'
import type { ActiveLineup, LineupPlayer, MapComparison } from '../../services/opponentReportApi'
import {
  inactiveSummary,
  inactiveTitle,
  leagueMatchesLabel,
  lifetimeLabel,
  lifetimeTitle,
  lineupHeading,
  lineupPlayerLabel,
  ourSampleNote,
  ourTitle,
  teamGamesSplitLabel,
} from './lineup'

const row = (overrides: Partial<MapComparison>): MapComparison => ({
  mapName: 'Nuke',
  theirGames: 0,
  theirWins: 0,
  theirWinRate: null,
  theirAvgRoundDiff: null,
  theirShare: 0,
  theirLastPlayedAtUtc: null,
  theirTrend: null,
  ourGames: 2,
  ourWins: 0,
  ourWinRate: 0,
  ourFaceitGames: 1,
  ourInternalGames: 1,
  poolStatus: 'Playable',
  advantage: 0,
  confidence: 'Low',
  prediction: 'Unknown',
  predictionReason: '',
  vetoScore: 10,
  recommendation: 'Neutral',
  vetoReasons: [],
  ...overrides,
})

const player: LineupPlayer = {
  playerId: 'p1',
  nickname: 'f0xelon',
  elo: 2100,
  skillLevel: 10,
  recentTeamGames: 8,
  teamGames: 14,
  lastTeamGameAtUtc: null,
}

describe('lifetime labels', () => {
  it('mirrors the match room aggregate and explains it on hover', () => {
    const lifetime = {
      players: 5,
      matches: 113,
      winRate: 52.4,
      avgKdRatio: 1.12,
      share: 22.6,
      experienced: true,
    }

    expect(lifetimeLabel(lifetime)).toBe('113 m. · K/D 1.12')
    expect(lifetimeTitle(lifetime, 'oni')).toContain('Aktywny skład: 5 graczy, 113 meczów lifetime')
    expect(lifetimeTitle(lifetime, 'oni')).toContain('Grają ją indywidualnie dużo')
  })

  it('shows a dash without data, also for older reports', () => {
    expect(lifetimeLabel(undefined)).toBe('—')
    expect(lifetimeLabel(null)).toBe('—')
    expect(lifetimeTitle(undefined, 'my')).toBeUndefined()
  })
})

describe('our sample note', () => {
  it('flags a tiny sample and explains it is not deciding', () => {
    const map = row({ ourLowSample: true, ourSmoothedWinRate: 35.7 })

    expect(ourSampleNote(map)).toBe('za mało danych')
    expect(ourTitle(map)).toContain('Wygładzony WR: 36%')
    expect(ourTitle(map)).toContain('by bilans wpływał na rekomendację')
  })

  it('stays silent for a meaningful sample or an older report without the flag', () => {
    expect(ourSampleNote(row({ ourGames: 9, ourLowSample: false }))).toBeNull()
    expect(ourSampleNote(row({}))).toBeNull()
    expect(ourSampleNote(row({ ourGames: 0, ourLowSample: true }))).toBe('brak danych')
  })
})

describe('lineup labels', () => {
  it('shows team games in the lineup window and the inactive history', () => {
    expect(lineupPlayerLabel(player, 10)).toBe('f0xelon (8/10)')
    expect(lineupPlayerLabel(player, 0)).toBe('f0xelon')
    expect(inactiveTitle(player)).toBe('Mecze drużynowe w oknie: 14, ostatni: brak')
    expect(inactiveTitle(player, { source: 'EseaSeason' })).toBe(
      'Mecze oficjalne drużyny w oknie: 14, ostatni: brak',
    )
  })
})

const lineup = (overrides: Partial<ActiveLineup>): ActiveLineup => ({
  basis: '',
  windowGames: 4,
  active: [player],
  inactive: [],
  ...overrides,
})

describe('ESEA season lineup', () => {
  it('declines "league match" in Polish', () => {
    expect(leagueMatchesLabel(1)).toBe('1 mecz ligowy')
    expect(leagueMatchesLabel(3)).toBe('3 mecze ligowe')
    expect(leagueMatchesLabel(5)).toBe('5 meczów ligowych')
    expect(leagueMatchesLabel(12)).toBe('12 meczów ligowych')
    expect(leagueMatchesLabel(22)).toBe('22 mecze ligowe')
    expect(leagueMatchesLabel(0)).toBe('0 meczów ligowych')
  })

  it('names the lineup by its source', () => {
    expect(lineupHeading(lineup({ source: 'EseaSeason', season: 'S59', windowGames: 1 }))).toBe(
      'Skład z sezonu ESEA S59 (1 mecz ligowy)',
    )
    expect(lineupHeading(lineup({ source: 'OfficialMatches' }))).toBe('Skład z meczów oficjalnych drużyny')
    expect(lineupHeading(lineup({}))).toBe('Aktywny skład')
  })

  it('labels the collapsed team members for a season lineup', () => {
    expect(inactiveSummary(lineup({ source: 'EseaSeason', inactive: [player, player] }))).toBe(
      'Pozostali członkowie drużyny FACEIT (nie grali w tym sezonie) (2)',
    )
    expect(inactiveSummary(lineup({ inactive: [player] }))).toBe(
      'Byli / rezerwowi (1) — pominięci w statystykach graczy',
    )
  })

  it('splits their map games into official and played-together ones', () => {
    const map = row({ theirGames: 10, theirOfficialGames: 4, theirTogetherGames: 6 })
    const season = lineup({ source: 'EseaSeason', officialMatches: 12 })

    expect(teamGamesSplitLabel(map, season)).toBe('ESEA 4 · razem 6')
    expect(teamGamesSplitLabel(map, lineup({ source: 'OfficialMatches', officialMatches: 3 }))).toBe(
      'ofic. 4 · razem 6',
    )
    expect(teamGamesSplitLabel(map, lineup({ source: 'TeamGames' }))).toBeNull()
    expect(teamGamesSplitLabel(row({ theirGames: 10 }), season)).toBeNull()
    expect(teamGamesSplitLabel(map, null)).toBeNull()
  })
})
