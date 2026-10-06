import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { AddFormModal } from '../../components/AddFormModal'
import { Button } from '../../components/ui/Button'
import { PageHeader } from '../../components/ui/PageHeader'
import { Tabs, type TabItem } from '../../components/ui/Tabs'
import { Modal } from '../../components/Modal'
import { VacationForm } from '../../features/availability/components/VacationForm'
import { VacationList } from '../../features/availability/components/VacationList'
import { WeeklyCalendar } from '../../features/availability/components/WeeklyCalendar'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { CreateEventForm } from '../../features/calendar/components/CreateEventForm'
import { EventList } from '../../features/calendar/components/EventList'

type CalendarTab = 'events' | 'availability'

const calendarTabs: readonly TabItem<CalendarTab>[] = [
  { id: 'events', label: 'Wydarzenia' },
  { id: 'availability', label: 'Dostępność' },
]

/** Merged calendar: events first (list + create), then the weekly availability grid; vacations live behind a button. The `?event=<id>` deep link opens the event on the events tab. */
export function CalendarPage() {
  const canManage = useIsCoachOrManager()
  const [searchParams, setSearchParams] = useSearchParams()
  const tab: CalendarTab = searchParams.get('tab') === 'availability' && !searchParams.has('event') ? 'availability' : 'events'
  const [isVacationsOpen, setIsVacationsOpen] = useState(false)
  const [isAddingVacation, setIsAddingVacation] = useState(false)

  function selectTab(next: CalendarTab) {
    const params = new URLSearchParams(searchParams)
    params.set('tab', next)
    setSearchParams(params, { replace: true })
  }

  return (
    <>
      <PageHeader
        title="Kalendarz"
        actions={
          <>
            <Button variant="secondary" onClick={() => setIsVacationsOpen(true)}>
              Urlopy
            </Button>
            {canManage && (
              <AddFormModal buttonLabel="+ Dodaj wydarzenie" title="Dodaj wydarzenie">
                {(close) => <CreateEventForm onDone={close} />}
              </AddFormModal>
            )}
          </>
        }
      />

      <Tabs tabs={calendarTabs} value={tab} onChange={selectTab} />

      {tab === 'events' ? <EventList /> : <WeeklyCalendar />}

      {isVacationsOpen && (
        <Modal title="Urlopy" onClose={() => setIsVacationsOpen(false)}>
          <div className="flex flex-col gap-3">
            <VacationList />
            <Button className="self-start" onClick={() => setIsAddingVacation(true)}>
              + Dodaj urlop
            </Button>
          </div>
        </Modal>
      )}

      {isAddingVacation && (
        <Modal title="Dodaj urlop" onClose={() => setIsAddingVacation(false)}>
          <VacationForm onDone={() => setIsAddingVacation(false)} />
        </Modal>
      )}
    </>
  )
}
