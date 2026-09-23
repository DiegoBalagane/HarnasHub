import { useState } from 'react'
import { useCreateEvent } from '../hooks/useCalendar'
import { EventForm } from './EventForm'

interface CreateEventFormProps {
  /** Called after a successful submit — e.g. to close the modal hosting this form. */
  onDone?: () => void
}

/** Coach/Manager-only form for scheduling a new calendar event. */
export function CreateEventForm({ onDone }: CreateEventFormProps) {
  const createEvent = useCreateEvent()
  // Bumped on a successful submit to remount EventForm with blank fields — its own state can't
  // reset itself since it doesn't know the mutation succeeded.
  const [formKey, setFormKey] = useState(0)

  return (
    <EventForm
      key={formKey}
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
