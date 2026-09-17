import type { AvailabilityStatus, EventType } from '../../services/calendarApi'

export const eventTypeLabels: Record<EventType, string> = {
  Training: 'Trening',
  PickupGame: 'Gra luźna',
  Match: 'Mecz',
  Tournament: 'Turniej',
  Scrim: 'Sparing',
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
