import type { AdvancedPlayer } from '../../services/advancedStatsApi'

/** A fully populated player row for the advanced stats tests. */
export function makePlayer(overrides: Partial<AdvancedPlayer> = {}): AdvancedPlayer {
  return {
    userId: 'u1',
    name: 'Alice',
    matches: 3,
    rounds: 60,
    openingWonT: 3,
    openingLostT: 1,
    openingWonCt: 0,
    openingLostCt: 0,
    tradeKills: 5,
    deaths: 10,
    tradedDeaths: 4,
    clutchAttempts: 2,
    clutchesWon: 1,
    bestClutchWon: 2,
    enemiesFlashed: 8,
    avgBlindSeconds: 1.5,
    teamFlashes: 1,
    utilityDamagePerMatch: 12.3,
    avgKast: 71,
    avgRating: 1.12,
    avgAdr: 80.5,
    ...overrides,
  }
}
