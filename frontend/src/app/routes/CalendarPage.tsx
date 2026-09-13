import { CreateEventForm } from '../../features/calendar/components/CreateEventForm'
import { EventList } from '../../features/calendar/components/EventList'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'

const coachRoles = new Set(['Coach', 'Manager'])

export function CalendarPage() {
  const role = useAuthStore((state) => state.role)
  const canManage = role !== null && coachRoles.has(role)

  return (
    <>
      <h1 className="text-2xl font-semibold">Kalendarz</h1>
      {canManage && <CreateEventForm />}
      <EventList />
    </>
  )
}
