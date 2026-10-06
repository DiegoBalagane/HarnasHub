import { describe, expect, it } from 'vitest'
import type { FaceitMatchPrefill } from '../../services/resultsApi'
import { linkSourceOf, opponentFaction, opponentNameOf, toDateTimeLocal } from './faceitPrefill'

function prefill(overrides: Partial<FaceitMatchPrefill> = {}): FaceitMatchPrefill {
  return {
    matchId: '1-abc',
    competitionName: 'Weekend Cup',
    category: 'Tournament',
    playedAtUtc: '2026-10-01T18:00:00Z',
    mapName: 'Mirage',
    ourFactionIndex: null,
    factions: [
      { name: 'team_Alpha', demoTeam: 'B', playerIds: ['a1'], nicknames: ['Alpha'], linkedOpponentName: null },
      { name: 'team_Charlie', demoTeam: 'A', playerIds: ['b1', 'b2'], nicknames: ['Charlie', 'Delta'], linkedOpponentName: 'Rivals' },
    ],
    ...overrides,
  }
}

describe('opponentFaction', () => {
  it('takes the faction that is not ours when the server found us', () => {
    expect(opponentFaction(prefill({ ourFactionIndex: 1 }), '')?.name).toBe('team_Alpha')
  })

  it('falls back to the faction that did not play as the picked demo team', () => {
    expect(opponentFaction(prefill(), 'B')?.name).toBe('team_Charlie')
    expect(opponentFaction(prefill(), 'A')?.name).toBe('team_Alpha')
  })

  it('returns null when nothing decides it', () => {
    expect(opponentFaction(prefill(), '')).toBeNull()
  })
})

describe('prefill helpers', () => {
  it('prefers an already linked opponent name and builds the nickname link source', () => {
    const faction = prefill().factions[1]

    expect(opponentNameOf(faction)).toBe('Rivals')
    expect(opponentNameOf(prefill().factions[0])).toBe('team_Alpha')
    expect(linkSourceOf(faction)).toBe('Charlie, Delta')
  })

  it('formats a timestamp for a datetime-local input', () => {
    const local = new Date('2026-10-01T18:05:00Z')
    const expected = `${local.getFullYear()}-${String(local.getMonth() + 1).padStart(2, '0')}-${String(local.getDate()).padStart(2, '0')}T${String(local.getHours()).padStart(2, '0')}:05`

    expect(toDateTimeLocal('2026-10-01T18:05:00Z')).toBe(expected)
    expect(toDateTimeLocal(null)).toBe('')
    expect(toDateTimeLocal('nonsense')).toBe('')
  })
})
