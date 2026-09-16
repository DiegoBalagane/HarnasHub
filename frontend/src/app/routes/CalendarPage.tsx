import { VacationForm } from '../../features/availability/components/VacationForm'
import { VacationList } from '../../features/availability/components/VacationList'
import { WeeklyCalendar } from '../../features/availability/components/WeeklyCalendar'
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

      <WeeklyCalendar />

      <section className="flex w-full flex-col gap-3">
        <h2 className="text-lg font-semibold">Urlopy</h2>
        <VacationForm />
        <VacationList />
      </section>

      <section className="flex w-full flex-col gap-3">
        <h2 className="text-lg font-semibold">Wydarzenia</h2>
        {canManage && <CreateEventForm />}
        <EventList />
      </section>
    </>
  )
}
