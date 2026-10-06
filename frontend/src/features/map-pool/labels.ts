import type { MapPoolStatus } from '../../services/mapPoolApi'

/** Selectable statuses in the order a coach thinks about them. */
export const mapPoolStatuses: MapPoolStatus[] = ['Core', 'Playable', 'Learning', 'Ban']

export const mapPoolStatusLabels: Record<MapPoolStatus, string> = {
  Core: 'Pewniak',
  Playable: 'Gramy',
  Learning: 'W przygotowaniu',
  Ban: 'Ban',
}

/** Tailwind classes for the status badge. */
export const mapPoolStatusClasses: Record<MapPoolStatus, string> = {
  Core: 'border-success-600 bg-success-950/50 text-success-300',
  Playable: 'border-info-600 bg-info-950/50 text-info-300',
  Learning: 'border-warning-600 bg-warning-950/50 text-warning-300',
  Ban: 'border-danger-700 bg-danger-950/50 text-danger-300',
}

/** Colored dot per recent-form outcome. */
export const formDotClasses: Record<'W' | 'L' | 'D', string> = {
  W: 'bg-success-500',
  L: 'bg-danger-500',
  D: 'bg-neutral-500',
}
