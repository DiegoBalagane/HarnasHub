import type { BuyType, InsightTone, RoundEndReason } from '../../services/matchAnalysisApi'

/** Polish names of buy types. */
export const buyTypeLabels: Record<BuyType, string> = {
  Pistol: 'Pistolówka',
  Eco: 'Eco',
  SemiEco: 'Semi-eco',
  Force: 'Force',
  Full: 'Pełny zakup',
}

/** Compact badges for buy types in the round grid. */
export const buyTypeShort: Record<BuyType, string> = {
  Pistol: 'P',
  Eco: 'E',
  SemiEco: 'SE',
  Force: 'F',
  Full: '$',
}

/** Polish descriptions of how a round ended. */
export const endReasonLabels: Record<RoundEndReason, string> = {
  Other: 'Inny',
  BombExploded: 'Wybuch bomby',
  BombDefused: 'Rozbrojenie',
  Elimination: 'Eliminacja',
  TimeExpired: 'Koniec czasu',
  Surrender: 'Poddanie',
}

/** Formats seconds as m:ss. */
export function formatRoundTime(seconds: number): string {
  const whole = Math.max(0, Math.floor(seconds))
  return `${Math.floor(whole / 60)}:${String(whole % 60).padStart(2, '0')}`
}
/** Subtle card tint per insight tone: a coloured left accent on a neutral card, so a list of problems doesn't read as a wall of red. */
export const insightToneStyles: Record<InsightTone, string> = {
  Negative: 'border-neutral-800 border-l-2 border-l-danger-500/70 bg-danger-950/10',
  Positive: 'border-neutral-800 border-l-2 border-l-success-500/70 bg-success-950/10',
  Neutral: 'border-neutral-800 bg-surface-card/40',
}

