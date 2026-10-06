import type { AvailabilityStatus, EventType } from '../../services/calendarApi'

export const eventTypeLabels: Record<EventType, string> = {
  Training: 'Trening',
  PickupGame: 'Gra luźna',
  Match: 'Mecz',
  Tournament: 'Turniej',
  Scrim: 'Sparing',
}

/** Text color per event type, so a training visibly stands out from a match/scrim/etc. at a glance. */
export const eventTypeColors: Record<EventType, string> = {
  Training: 'text-purple-400',
  PickupGame: 'text-info-400',
  Match: 'text-primary-400',
  Tournament: 'text-warning-400',
  Scrim: 'text-success-400',
}

/** Left-border accent per event type, for list rows/chips. */
export const eventTypeBorderColors: Record<EventType, string> = {
  Training: 'border-l-purple-400',
  PickupGame: 'border-l-info-400',
  Match: 'border-l-primary-400',
  Tournament: 'border-l-warning-400',
  Scrim: 'border-l-success-400',
}

export const availabilityLabels: Record<AvailabilityStatus | 'NotSet', string> = {
  Available: 'Dostępny',
  Maybe: 'Niepewne',
  Unavailable: 'Niedostępny',
  NotSet: 'Brak odpowiedzi',
}

export const availabilityColors: Record<AvailabilityStatus | 'NotSet', string> = {
  Available: 'text-success-400',
  Maybe: 'text-warning-400',
  Unavailable: 'text-danger-400',
  NotSet: 'text-neutral-500',
}
