import { useState } from 'react'
import { Modal } from '../../components/Modal'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { CreateEventForm } from '../../features/calendar/components/CreateEventForm'
import { EventList } from '../../features/calendar/components/EventList'

export function EventsPage() {
  const canManage = useIsCoachOrManager()
  const [isAdding, setIsAdding] = useState(false)

  return (
    <>
      <h1 className="text-2xl font-semibold">Wydarzenia</h1>

      <EventList />

      {canManage && (
        <button
          type="button"
          onClick={() => setIsAdding(true)}
          className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500"
        >
          + Dodaj wydarzenie
        </button>
      )}

      {isAdding && (
        <Modal title="Dodaj wydarzenie" onClose={() => setIsAdding(false)}>
          <CreateEventForm onDone={() => setIsAdding(false)} />
        </Modal>
      )}
    </>
  )
}
