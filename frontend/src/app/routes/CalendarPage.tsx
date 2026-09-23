import { useState } from 'react'
import { Modal } from '../../components/Modal'
import { VacationForm } from '../../features/availability/components/VacationForm'
import { VacationList } from '../../features/availability/components/VacationList'
import { WeeklyCalendar } from '../../features/availability/components/WeeklyCalendar'

export function CalendarPage() {
  const [isAddingVacation, setIsAddingVacation] = useState(false)

  return (
    <>
      <h1 className="text-2xl font-semibold">Kalendarz</h1>

      <WeeklyCalendar />

      <section className="flex w-full flex-col gap-3">
        <h2 className="text-lg font-semibold">Urlopy</h2>
        <VacationList />
        <button
          type="button"
          onClick={() => setIsAddingVacation(true)}
          className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500"
        >
          + Dodaj urlop
        </button>
      </section>

      {isAddingVacation && (
        <Modal title="Dodaj urlop" onClose={() => setIsAddingVacation(false)}>
          <VacationForm onDone={() => setIsAddingVacation(false)} />
        </Modal>
      )}
    </>
  )
}
