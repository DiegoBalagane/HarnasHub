/** Text colour for a score: win green, loss red, draw neutral (a 12:12 must not read as a loss). */
export function scoreToneClass(ourScore: number, opponentScore: number): string {
  if (ourScore > opponentScore) return 'text-success-400'
  if (ourScore < opponentScore) return 'text-danger-400'
  return 'text-neutral-300'
}
