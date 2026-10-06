import type { SidedMapPosition } from '../../services/mapStrategyApi'

/** Two same-player pins closer than this (radar fraction, per axis) count as exactly overlapping. */
const overlapThreshold = 0.012
/** Horizontal shift (px) applied to each of two overlapping pins so both stay visible and grabbable. */
export const overlapOffsetPx = 9

/** Returns a horizontal pixel offset per position id: when one player has a T and a CT pin on (nearly) the same
 * spot, T shifts left and CT right; every other pin keeps offset 0. */
export function overlapOffsets(positions: SidedMapPosition[]): Map<string, number> {
  const offsets = new Map<string, number>()

  for (const position of positions) {
    const twin = positions.find(
      (other) =>
        other.userId === position.userId &&
        other.side !== position.side &&
        Math.abs(other.x - position.x) < overlapThreshold &&
        Math.abs(other.y - position.y) < overlapThreshold,
    )

    if (twin) {
      offsets.set(position.id, position.side === 'T' ? -overlapOffsetPx : overlapOffsetPx)
    }
  }

  return offsets
}
