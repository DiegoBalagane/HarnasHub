import type { DayStatus } from '../../services/availabilityApi'

export const dayStatusLabels: Record<DayStatus, string> = {
  Available: 'Dostępny',
  PartiallyAvailable: 'Częściowo dostępny',
  Off: 'Off',
  NotSet: 'Brak odpowiedzi',
}

/** Badge classes per status; vacation days use `vacationColor` instead. */
export const dayStatusColors: Record<DayStatus, string> = {
  Available: 'border-success-700 bg-success-950 text-success-300',
  PartiallyAvailable: 'border-warning-700 bg-warning-950 text-warning-300',
  Off: 'border-neutral-700 bg-neutral-900 text-neutral-500 line-through',
  NotSet: 'border-neutral-800 bg-neutral-950 text-neutral-600',
}

export const vacationColor = 'border-purple-700 bg-purple-950 text-purple-300'
export const vacationLabel = 'Urlop'
export const vacationIcon = '🧳'

export const weekdayLabels = ['Pon', 'Wt', 'Śr', 'Czw', 'Pt', 'Sob', 'Nd']
