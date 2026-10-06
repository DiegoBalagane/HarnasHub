import { describe, expect, it } from 'vitest'
import type { SidedMapPosition } from '../../services/mapStrategyApi'
import { overlapOffsetPx, overlapOffsets } from './pinLayout'

function pin(id: string, userId: string, side: 'T' | 'CT', x: number, y: number): SidedMapPosition {
  return {
    id, userId, side, x, y, displayName: userId, inGameNickname: null, teamRole: null,
    pinColor: null, pinMark: null, label: null, note: null,
  }
}

describe('overlapOffsets', () => {
  it('separates one player T and CT pins sitting on the same spot', () => {
    const offsets = overlapOffsets([pin('a', 'u1', 'T', 0.5, 0.5), pin('b', 'u1', 'CT', 0.5, 0.5)])
    expect(offsets.get('a')).toBe(-overlapOffsetPx)
    expect(offsets.get('b')).toBe(overlapOffsetPx)
  })

  it('leaves distant pins and different players alone', () => {
    const offsets = overlapOffsets([
      pin('a', 'u1', 'T', 0.2, 0.2),
      pin('b', 'u1', 'CT', 0.7, 0.7),
      pin('c', 'u2', 'CT', 0.2, 0.2),
    ])
    expect(offsets.size).toBe(0)
  })
})
