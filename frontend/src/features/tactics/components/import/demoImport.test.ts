import { describe, expect, it } from 'vitest'
import type { DemoNade, DemoNadeRound } from '../../../../services/tacticsApi'
import { formatRoundTime, grenadesForSide, groupByThrower, toImportInputs } from './demoImport'

function nade(id: number, overrides: Partial<DemoNade> = {}): DemoNade {
  return {
    id,
    type: 'Smoke',
    throwerName: 'Zed',
    throwerSteamId: '1',
    side: 'T',
    throwX: 1,
    throwY: 2,
    landX: 3,
    landY: 4,
    secondsIntoRound: 10,
    ...overrides,
  }
}

describe('formatRoundTime', () => {
  it('formats seconds as m:ss and floors fractions', () => {
    expect(formatRoundTime(14.7)).toBe('0:14')
    expect(formatRoundTime(75)).toBe('1:15')
  })

  it('clamps negative values to zero', () => {
    expect(formatRoundTime(-3)).toBe('0:00')
  })
})

describe('grenadesForSide', () => {
  it('returns only the requested side and tolerates a missing round', () => {
    const round = { grenades: [nade(1, { side: 'T' }), nade(2, { side: 'CT' })] } as DemoNadeRound
    expect(grenadesForSide(round, 'CT').map((g) => g.id)).toEqual([2])
    expect(grenadesForSide(undefined, 'T')).toEqual([])
  })
})

describe('groupByThrower', () => {
  it('groups by SteamID, keeps throw order and sorts players by name', () => {
    const groups = groupByThrower([
      nade(1, { throwerName: 'Zed', throwerSteamId: '1' }),
      nade(2, { throwerName: 'Adam', throwerSteamId: '2' }),
      nade(3, { throwerName: 'Zed', throwerSteamId: '1' }),
    ])
    expect(groups.map((g) => g.throwerName)).toEqual(['Adam', 'Zed'])
    expect(groups[1].grenades.map((g) => g.id)).toEqual([1, 3])
  })

  it('falls back to the nick when SteamID is unknown', () => {
    const groups = groupByThrower([nade(1, { throwerSteamId: null }), nade(2, { throwerSteamId: null })])
    expect(groups).toHaveLength(1)
    expect(groups[0].key).toBe('Zed')
  })
})

describe('toImportInputs', () => {
  it('keeps only selected grenades and strips ids/sides', () => {
    const result = toImportInputs([nade(1), nade(2, { type: 'Flash' })], new Set([2]))
    expect(result).toEqual([
      { type: 'Flash', throwerName: 'Zed', throwX: 1, throwY: 2, landX: 3, landY: 4, secondsIntoRound: 10 },
    ])
  })
})
