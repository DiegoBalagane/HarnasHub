// Written out in full (no string interpolation) so Tailwind's scanner keeps these classes in the build.
const pinPalette = [
  'bg-red-500',
  'bg-sky-500',
  'bg-emerald-500',
  'bg-amber-500',
  'bg-fuchsia-500',
  'bg-cyan-500',
] as const

/** Picks a stable pin colour for a player so the same person keeps the same dot across maps and sides. */
export function pinColorFor(seed: string): string {
  let hash = 0

  for (let index = 0; index < seed.length; index += 1) {
    hash = (hash * 31 + seed.charCodeAt(index)) % 1_000_003
  }

  return pinPalette[hash % pinPalette.length]
}
