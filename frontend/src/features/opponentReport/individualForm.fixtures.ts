import type { MapName } from '../../services/nadesApi'
import type { PlayerForm, PlayerMapForm } from '../../services/opponentReportApi'

/** Test fixture: a player map row with the given games (half team, half solo). */
export function makeMap(mapName: MapName, games: number, overrides: Partial<PlayerMapForm> = {}): PlayerMapForm {
  return {
    mapName,
    games,
    share: games * 5,
    wins: Math.floor(games / 2),
    winRate: 50,
    kdRatio: 1.1,
    adr: 82.4,
    headshotPercent: 48,
    lastPlayedAtUtc: '2026-09-30T12:00:00Z',
    teamGames: Math.floor(games / 2),
    soloGames: Math.ceil(games / 2),
    teamWinRate: 50,
    soloWinRate: 50,
    teamKdRatio: 1.0,
    soloKdRatio: 1.2,
    ...overrides,
  }
}

/** Test fixture: a linked player with sensible defaults. */
export function makePlayer(overrides: Partial<PlayerForm> = {}): PlayerForm {
  return {
    playerId: 'p1',
    nickname: 'f0xelon',
    elo: 2450,
    skillLevel: 10,
    games: 40,
    teamGames: 10,
    soloGames: 30,
    winRate: 55,
    kdRatio: 1.21,
    adr: 85,
    headshotPercent: 50,
    lastPlayedAtUtc: '2026-09-30T12:00:00Z',
    recentForm: null,
    maps: [],
    ...overrides,
  }
}
