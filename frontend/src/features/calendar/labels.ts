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

/** Tinted block background + border per event type, for month chips and week blocks. */
export const eventTypeBlockColors: Record<EventType, string> = {
  Training: 'border-purple-400/40 bg-purple-400/15 hover:bg-purple-400/25',
  PickupGame: 'border-info-400/40 bg-info-400/15 hover:bg-info-400/25',
  Match: 'border-primary-400/40 bg-primary-400/15 hover:bg-primary-400/25',
  Tournament: 'border-warning-400/40 bg-warning-400/15 hover:bg-warning-400/25',
  Scrim: 'border-success-400/40 bg-success-400/15 hover:bg-success-400/25',
}
