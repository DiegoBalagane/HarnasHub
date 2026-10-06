import { describe, expect, it } from 'vitest'
import type { ReplayGrenade, RoundReplay } from '../../../services/replayApi'
import { buildSeries, clampTime, deathSeconds, grenadeStateAt, positionAt } from './replayMath'

function replay(overrides: object): RoundReplay {
  return { durationSeconds: 4, players: [{}, {}], frames: [], kills: [], grenades: [], ...overrides } as unknown as RoundReplay
}

describe('buildSeries / positionAt', () => {
  const series = buildSeries(
    replay({
      frames: [
        { second: 0, players: [{ player: 0, x: 0, y: 0 }] },
        { second: 1, players: [{ player: 0, x: 10, y: 20 }] },
        { second: 99, players: [{ player: 0, x: 5, y: 5 }] },
      ],
    }),
  )

  it('creates one series per player with NaN for unsampled seconds', () => {
    expect(series).toHaveLength(2)
    expect(Number.isNaN(series[1].xs[0])).toBe(true)
  })

  it('interpolates linearly between neighbouring seconds', () => {
    expect(positionAt(series[0], 0.5)).toEqual({ x: 5, y: 10 })
  })

  it('holds the last sample when the next second is missing', () => {
    expect(positionAt(series[0], 1.5)).toEqual({ x: 10, y: 20 })
  })

  it('returns null for unsampled or out-of-range times', () => {
    expect(positionAt(series[1], 0)).toBeNull()
    expect(positionAt(series[0], -1)).toBeNull()
    expect(positionAt(series[0], 100)).toBeNull()
  })
})

describe('deathSeconds', () => {
  it('records only the first death per victim', () => {
    const deaths = deathSeconds(
      replay({ kills: [{ victim: 1, second: 7 }, { victim: 1, second: 9 }, { victim: 0, second: 3 }] }),
    )
    expect(deaths.get(1)).toBe(7)
    expect(deaths.get(0)).toBe(3)
  })
})

describe('grenadeStateAt', () => {
  const grenade = {
    throwSecond: 10,
    detonateSecond: 12,
    endSecond: 22,
    throwX: 0,
    throwY: 0,
    landX: 100,
    landY: 50,
  } as ReplayGrenade

  it('is hidden outside its lifetime or without a landing point', () => {
    expect(grenadeStateAt(grenade, 9).phase).toBe('hidden')
    expect(grenadeStateAt(grenade, 23).phase).toBe('hidden')
    expect(grenadeStateAt({ ...grenade, landX: null }, 11).phase).toBe('hidden')
  })

  it('flies along the throw-to-landing line before detonation', () => {
    expect(grenadeStateAt(grenade, 11)).toEqual({ phase: 'flying', x: 50, y: 25 })
  })

  it('is active after detonation with the remaining fraction of the effect', () => {
    expect(grenadeStateAt(grenade, 17)).toEqual({ phase: 'active', x: 100, y: 50, remaining: 0.5 })
  })
})

describe('clampTime', () => {
  it('clamps into [0, duration]', () => {
    expect(clampTime(-1, 10)).toBe(0)
    expect(clampTime(11, 10)).toBe(10)
    expect(clampTime(4, 10)).toBe(4)
  })
})
