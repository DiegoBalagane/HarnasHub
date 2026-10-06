import { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Modal } from '../../../components/Modal'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { computeDaySummary } from '../../availability/daySummary'
import { useWeekAvailability } from '../../availability/hooks/useAvailability'
import { toIsoDate } from '../../availability/weekDates'
import { formatRangeLabel, shiftAnchor, startOfWeekIso, toStartsAtUtc, weekDaysOf, type CalendarView } from '../calendarGrid'
import { useUpcomingEvents } from '../hooks/useCalendar'
import { useIsDesktop } from '../hooks/useIsDesktop'
import { AgendaView } from './calendar/AgendaView'
import { CalendarHeader } from './calendar/CalendarHeader'
import { EventDetailsPanel } from './calendar/EventDetailsPanel'
import { MonthView } from './calendar/MonthView'
import { WeekView, type DayAvailabilityCounts } from './calendar/WeekView'
import { CreateEventForm } from './CreateEventForm'

/** Default start hour for an event created by clicking a whole day in the month view. */
const defaultNewEventHour = 18

/** Month / Week / Agenda calendar of events; clicking an event opens its details, Coach/Manager can click an empty day/slot to create one. */
export function EventCalendar() {
  const canManage = useIsCoachOrManager()
  const isDesktop = useIsDesktop()
  const [searchParams, setSearchParams] = useSearchParams()
  const linkedEventId = searchParams.get('event')
  const todayIso = toIsoDate(new Date())
  const [viewOverride, setViewOverride] = useState<CalendarView | null>(null)
  const view: CalendarView = viewOverride ?? (isDesktop ? 'month' : 'agenda')
  const [anchorIso, setAnchorIso] = useState(todayIso)
  const [selectedEventId, setSelectedEventId] = useState<string | null>(linkedEventId)
  const [createStart, setCreateStart] = useState<string | null>(null)
  const { data: events = [], isLoading, isError } = useUpcomingEvents(true)
  const selectedEvent = events.find((event) => event.id === selectedEventId) ?? null
  const weekStart = startOfWeekIso(anchorIso)
  const { data: weekData } = useWeekAvailability(weekStart)

  // A dashboard/other-page link lands here with ?event=<id>: jump to that event's date, open its details, consume the param.
  useEffect(() => {
    const linked = linkedEventId ? events.find((event) => event.id === linkedEventId) : undefined
    if (!linked) return

    setSelectedEventId(linked.id)
    setAnchorIso(toIsoDate(new Date(linked.startsAtUtc)))
    const nextParams = new URLSearchParams(searchParams)
    nextParams.delete('event')
    setSearchParams(nextParams, { replace: true })
    // Runs once the target event is present in the fetched list; the id is consumed immediately after.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [linkedEventId, events])

  const availability = useMemo<DayAvailabilityCounts | undefined>(() => {
    if (!weekData) return undefined

    return Object.fromEntries(
      weekDaysOf(weekStart).map((date) => {
        const { main } = computeDaySummary(weekData.members, date)
        return [date, { available: main.availableCount, total: main.totalCount }]
      }),
    )
  }, [weekData, weekStart])

  const closeDetails = useCallback(() => setSelectedEventId(null), [])
  const createOnDay = canManage ? (date: string) => setCreateStart(toStartsAtUtc(date, defaultNewEventHour)) : undefined
  const createAt = canManage ? (date: string, hour: number) => setCreateStart(toStartsAtUtc(date, hour)) : undefined
  const openDay = (date: string) => {
    setAnchorIso(date)
    setViewOverride('week')
  }

  return (
    <div className="flex flex-1 flex-col gap-3 lg:h-[calc(100dvh-13rem)] lg:flex-none">
      <CalendarHeader
        label={formatRangeLabel(anchorIso, view)}
        view={view}
        onViewChange={setViewOverride}
        onPrev={() => setAnchorIso(shiftAnchor(anchorIso, view, -1))}
        onNext={() => setAnchorIso(shiftAnchor(anchorIso, view, 1))}
        onToday={() => setAnchorIso(todayIso)}
      />

      {isLoading && <p className="text-neutral-400">Ładowanie kalendarza…</p>}
      {isError && <p className="text-danger-400">Nie udało się pobrać wydarzeń.</p>}

      {view === 'month' && (
        <MonthView
          anchorIso={anchorIso}
          todayIso={todayIso}
          events={events}
          onCreateOnDay={createOnDay}
          onSelectEvent={setSelectedEventId}
          onOpenDay={openDay}
        />
      )}
      {view === 'week' && (
        <WeekView
          anchorIso={anchorIso}
          todayIso={todayIso}
          events={events}
          availability={availability}
          onCreateAt={createAt}
          onSelectEvent={setSelectedEventId}
        />
      )}
      {view === 'agenda' && (
        <AgendaView anchorIso={anchorIso} todayIso={todayIso} events={events} onSelectEvent={setSelectedEventId} />
      )}

      {selectedEvent && <EventDetailsPanel key={selectedEvent.id} event={selectedEvent} onClose={closeDetails} />}

      {createStart && (
        <Modal title="Dodaj wydarzenie" onClose={() => setCreateStart(null)}>
          <CreateEventForm initialStartsAtUtc={createStart} onDone={() => setCreateStart(null)} />
        </Modal>
      )}
    </div>
  )
}
