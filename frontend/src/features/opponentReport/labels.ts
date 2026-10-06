import type { ConfidenceLevel, InsightSeverity, OpponentVetoPrediction } from '../../services/opponentReportApi'

export const confidenceLabels: Record<ConfidenceLevel, string> = {
  Low: 'niska',
  Medium: 'średnia',
  High: 'wysoka',
}

export const predictionLabels: Record<OpponentVetoPrediction, string> = {
  Ban: 'ban',
  Pick: 'pick',
  Neutral: '—',
  Unknown: '?',
}

/** Tailwind classes of a TL;DR point by severity. */
export const severityClasses: Record<InsightSeverity, string> = {
  High: 'border-success-600 bg-success-950/40 text-success-200',
  Warning: 'border-warning-600 bg-warning-950/30 text-warning-200',
  Info: 'border-neutral-700 bg-neutral-900 text-neutral-200',
}

/** Tailwind text colour of an advantage in percentage points: green ours, red theirs, grey when close. */
export function advantageClass(advantage: number): string {
  if (advantage >= 10) return 'text-success-400'
  if (advantage >= 3) return 'text-success-300/80'
  if (advantage <= -10) return 'text-danger-400'
  if (advantage <= -3) return 'text-danger-300/80'
  return 'text-neutral-400'
}

/** A percentage for display, "—" when unknown. */
export function formatPercent(value: number | null): string {
  return value === null ? '—' : `${Math.round(value)}%`
}

/** Percentage points with an explicit sign. */
export function formatSigned(value: number): string {
  const rounded = Math.round(value)
  return rounded > 0 ? `+${rounded}` : `${rounded}`
}
