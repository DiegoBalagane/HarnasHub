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
  PickupGame: 'text-blue-400',
  Match: 'text-red-400',
  Tournament: 'text-yellow-400',
  Scrim: 'text-green-400',
}

/** Left-border accent per event type, for list rows/chips. */
export const eventTypeBorderColors: Record<EventType, string> = {
  Training: 'border-l-purple-400',
  PickupGame: 'border-l-blue-400',
  Match: 'border-l-red-400',
  Tournament: 'border-l-yellow-400',
  Scrim: 'border-l-green-400',
}

export const availabilityLabels: Record<AvailabilityStatus | 'NotSet', string> = {
  Available: 'Dostępny',
  Maybe: 'Niepewne',
  Unavailable: 'Niedostępny',
  NotSet: 'Brak odpowiedzi',
}

export const availabilityColors: Record<AvailabilityStatus | 'NotSet', string> = {
  Available: 'text-green-400',
  Maybe: 'text-yellow-400',
  Unavailable: 'text-red-400',
  NotSet: 'text-neutral-500',
}
