import type { ReplayGrenade, RoundReplay } from '../../../services/replayApi'

/** One player's per-second radar position as flat typed arrays (NaN = not sampled/alive that second). */
export interface PlayerSeries {
  xs: Float32Array
  ys: Float32Array
}

/** An interpolated point on the radar. */
export interface ReplayPoint {
  x: number
  y: number
}

/** Unpacks the frames once into per-player typed arrays, so drawing a frame never searches lists. */
export function buildSeries(replay: RoundReplay): PlayerSeries[] {
  const length = replay.durationSeconds + 1
  const series = replay.players.map(() => ({
    xs: new Float32Array(length).fill(Number.NaN),
    ys: new Float32Array(length).fill(Number.NaN),
  }))

  for (const frame of replay.frames) {
    if (frame.second < 0 || frame.second >= length) continue
    for (const state of frame.players) {
      const target = series[state.player]
      if (!target) continue
      target.xs[frame.second] = state.x
      target.ys[frame.second] = state.y
    }
  }

  return series
}

/** Position at fractional time `t`: linear between the two neighbouring seconds, held at the last sample when the next
 * one is missing (the track ends at death), null when the player isn't sampled at floor(t). */
export function positionAt(series: PlayerSeries, t: number): ReplayPoint | null {
  const second = Math.floor(t)
  if (second < 0 || second >= series.xs.length) return null

  const x0 = series.xs[second]
  const y0 = series.ys[second]
  if (Number.isNaN(x0)) return null

  const x1 = series.xs[second + 1]
  const y1 = series.ys[second + 1]
  if (x1 === undefined || Number.isNaN(x1)) return { x: x0, y: y0 }

  const f = t - second
  return { x: x0 + (x1 - x0) * f, y: y0 + (y1 - y0) * f }
}

/** Second at which each player died (first death only), keyed by player index. */
export function deathSeconds(replay: RoundReplay): Map<number, number> {
  const deaths = new Map<number, number>()
  for (const kill of replay.kills) {
    if (!deaths.has(kill.victim)) deaths.set(kill.victim, kill.second)
  }
  return deaths
}

/** Where a grenade is at time `t`: flying along the throw→landing line, its effect active, or not on the map. */
export type GrenadeState =
  | { phase: 'hidden' }
  | { phase: 'flying'; x: number; y: number }
  | { phase: 'active'; x: number; y: number; remaining: number }

/** Resolves a grenade's state at `t` (fraction of the effect remaining in [0,1] when active). */
export function grenadeStateAt(grenade: ReplayGrenade, t: number): GrenadeState {
  if (grenade.landX === null || grenade.landY === null || t < grenade.throwSecond || t > grenade.endSecond) {
    return { phase: 'hidden' }
  }

  if (t < grenade.detonateSecond) {
    const startX = grenade.throwX ?? grenade.landX
    const startY = grenade.throwY ?? grenade.landY
    const span = Math.max(0.001, grenade.detonateSecond - grenade.throwSecond)
    const f = (t - grenade.throwSecond) / span
    return {
      phase: 'flying',
      x: startX + (grenade.landX - startX) * f,
      y: startY + (grenade.landY - startY) * f,
    }
  }

  const effect = Math.max(0.001, grenade.endSecond - grenade.detonateSecond)
  return {
    phase: 'active',
    x: grenade.landX,
    y: grenade.landY,
    remaining: 1 - (t - grenade.detonateSecond) / effect,
  }
}

/** Clamps a playback time into [0, duration]. */
export function clampTime(t: number, duration: number): number {
  return Math.min(Math.max(0, t), duration)
}
