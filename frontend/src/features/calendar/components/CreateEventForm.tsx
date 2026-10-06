import { useState } from 'react'
import { useCreateEvent } from '../hooks/useCalendar'
import { EventForm } from './EventForm'

interface CreateEventFormProps {
  /** Called after a successful submit — e.g. to close the modal hosting this form. */
  onDone?: () => void
  /** Optional UTC ISO start to prefill the form with (e.g. from a clicked calendar day/slot). */
  initialStartsAtUtc?: string
}

/** Coach/Manager-only form for scheduling a new calendar event. */
export function CreateEventForm({ onDone, initialStartsAtUtc }: CreateEventFormProps) {
  const createEvent = useCreateEvent()
  // Bumped on a successful submit to remount EventForm with blank fields — its own state can't
  // reset itself since it doesn't know the mutation succeeded.
  const [formKey, setFormKey] = useState(0)

  return (
    <EventForm
      key={formKey}
      initialValues={
        initialStartsAtUtc
          ? {
              title: '',
              type: 'Training',
              startsAtUtc: initialStartsAtUtc,
              endsAtUtc: null,
              location: null,
              url: null,
              opponent: null,
            }
          : undefined
      }
      onSubmit={(payload) =>
        createEvent.mutate(payload, {
          onSuccess: () => {
            setFormKey((current) => current + 1)
            onDone?.()
          },
        })
      }
      isPending={createEvent.isPending}
      isError={createEvent.isError}
      submitLabel="Dodaj wydarzenie"
    />
  )
}
