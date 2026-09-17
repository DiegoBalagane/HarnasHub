import { useState } from 'react'
import { useCreateEvent } from '../hooks/useCalendar'
import { EventForm } from './EventForm'

/** Coach/Manager-only form for scheduling a new calendar event. */
export function CreateEventForm() {
  const createEvent = useCreateEvent()
  // Bumped on a successful submit to remount EventForm with blank fields — its own state can't
  // reset itself since it doesn't know the mutation succeeded.
  const [formKey, setFormKey] = useState(0)

  return (
    <div className="flex w-full max-w-xl flex-col gap-3 rounded-md border border-neutral-800 p-4">
      <h2 className="font-medium">Dodaj wydarzenie</h2>
      <EventForm
        key={formKey}
        onSubmit={(payload) =>
          createEvent.mutate(payload, { onSuccess: () => setFormKey((current) => current + 1) })
        }
        isPending={createEvent.isPending}
        isError={createEvent.isError}
        submitLabel="Dodaj wydarzenie"
      />
    </div>
  )
}
