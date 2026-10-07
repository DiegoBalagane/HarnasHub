import { useCallback, useMemo, useState } from 'react'
import type { CalendarEvent } from '../../../services/calendarApi'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useUpcomingEvents } from '../../calendar/hooks/useCalendar'
import { useIsDesktop } from '../../calendar/hooks/useIsDesktop'
import { eligibleBulkDays } from '../bulkDays'
import { useWeekAvailability } from '../hooks/useAvailability'
import { compareSections, sectionOf } from '../rosterSections'
import { addDaysIso, buildWeekDates, entryFor, toIsoDate } from '../weekDates'
import { DayAvailabilityEditor } from './DayAvailabilityEditor'
import { BulkAvailabilityDialog } from './week/BulkAvailabilityDialog'
import { CellPopover } from './week/CellPopover'
import { RosterFooter } from './week/RosterFooter'
import { WeekGrid } from './week/WeekGrid'
import { WeekToolbar, type AvailabilityTab } from './week/WeekToolbar'

const rangeFormatter = new Intl.DateTimeFormat('pl-PL', { day: '2-digit', month: '2-digit' })
const dayArrowClass = 'rounded-md border border-neutral-700 px-3 py-1 text-sm text-neutral-300 hover:border-neutral-500'

/** Weekly availability: compact day headers with fill bars, slim status pills, a cell popover editor and a one-line coach footer. On phones it shows a single day with prev/next arrows. */
export function WeeklyCalendar() {
  const currentUserId = useAuthStore((state) => state.userId)
  const isDesktop = useIsDesktop()
  // A rolling window starting today, not the Monday of the calendar week — otherwise "Upcoming" on a
  // Sunday would show mostly days that already passed instead of what's actually coming up.
  const currentWeekStart = useMemo(() => toIsoDate(new Date()), [])
  const [tab, setTab] = useState<AvailabilityTab>('upcoming')
  const [weekStart, setWeekStart] = useState(currentWeekStart)
  const [dayIndex, setDayIndex] = useState(0)
  const [cell, setCell] = useState<{ date: string; rect: DOMRect } | null>(null)
  const [isBulkOpen, setIsBulkOpen] = useState(false)
  const { data, isLoading, isError } = useWeekAvailability(weekStart)
  const { data: events } = useUpcomingEvents()

  const weekDates = useMemo(() => buildWeekDates(weekStart), [weekStart])
  const todayIso = toIsoDate(new Date())
  const isHistory = tab === 'history'
  const visibleDates = useMemo(() => (isDesktop ? weekDates : [weekDates[dayIndex]]), [isDesktop, weekDates, dayIndex])

  const eventsByDate = useMemo(() => {
    const grouped = new Map<string, CalendarEvent[]>()
    for (const event of events ?? []) {
      const key = toIsoDate(new Date(event.startsAtUtc))
      grouped.set(key, [...(grouped.get(key) ?? []), event])
    }
    return grouped
  }, [events])

  const members = useMemo(
    () =>
      [...(data?.members ?? [])].sort((left, right) => {
        // Main squad above the bench, Coach always last, matching the backend's own ordering.
        const sectionDiff = compareSections(left, right)
        if (sectionDiff !== 0) return sectionDiff
        return (left.inGameNickname ?? left.displayName).localeCompare(right.inGameNickname ?? right.displayName, 'pl')
      }),
    [data],
  )
  const players = useMemo(() => members.filter((member) => sectionOf(member) !== 'Coach'), [members])
  const coaches = useMemo(() => members.filter((member) => sectionOf(member) === 'Coach'), [members])
  const myRow = members.find((member) => member.userId === currentUserId)

  const closeCell = useCallback(() => setCell(null), [])
  // Past days live exclusively in the History tab (read-only), matching the backend's not-in-the-past edit rule.
  const canEditDate = useCallback((date: string) => !isHistory && date >= todayIso, [isHistory, todayIso])
  const openCell = useCallback((date: string, rect: DOMRect) => setCell({ date, rect }), [])

  function goToWeek(next: string, nextDayIndex = 0) {
    setWeekStart(next)
    setDayIndex(nextDayIndex)
    setCell(null)
  }

  function switchTab(next: AvailabilityTab) {
    setTab(next)
    goToWeek(next === 'history' ? addDaysIso(currentWeekStart, -7) : currentWeekStart)
  }

  const canGoBack = isHistory || weekStart > currentWeekStart
  const canGoForward = !isHistory || addDaysIso(weekStart, 7) < currentWeekStart

  function stepDay(delta: number) {
    const next = dayIndex + delta
    if (next < 0) {
      if (canGoBack) goToWeek(addDaysIso(weekStart, -7), 6)
    } else if (next > 6) {
      if (canGoForward) goToWeek(addDaysIso(weekStart, 7), 0)
    } else {
      setDayIndex(next)
      setCell(null)
    }
  }

  const rangeLabel = `${rangeFormatter.format(new Date(`${weekDates[0]}T00:00`))} – ${rangeFormatter.format(new Date(`${weekDates[6]}T00:00`))}`
  const hasData = !isLoading && !isError && members.length > 0
  const rowProps = {
    dates: visibleDates,
    currentUserId,
    canEditDate,
    activeDate: cell?.date ?? null,
    onCellOpen: openCell,
    onBulk: myRow && !isHistory ? () => setIsBulkOpen(true) : undefined,
  }

  return (
    <section className="flex w-full flex-col gap-2 rounded-md border border-neutral-800 p-3 lg:h-[calc(100dvh-13rem)]">
      <WeekToolbar
        tab={tab}
        onTabChange={switchTab}
        rangeLabel={rangeLabel}
        canGoBack={canGoBack}
        canGoForward={canGoForward}
        onPrev={() => goToWeek(addDaysIso(weekStart, -7))}
        onNext={() => goToWeek(addDaysIso(weekStart, 7))}
        onToday={() => goToWeek(currentWeekStart)}
      />

      {isHistory && <p className="text-xs text-neutral-500">Historia jest tylko do odczytu — edycja dotyczy dzisiaj i kolejnych dni.</p>}
      {isLoading && <p className="text-neutral-400">Ładowanie dostępności…</p>}
      {isError && <p className="text-danger-400">Nie udało się pobrać dostępności.</p>}
      {!isLoading && !isError && members.length === 0 && <p className="text-neutral-400">Brak członków drużyny do wyświetlenia.</p>}

      {hasData && !isDesktop && (
        <div className="flex items-center justify-between gap-2">
          <button type="button" aria-label="Poprzedni dzień" className={dayArrowClass} onClick={() => stepDay(-1)}>
            ‹
          </button>
          <span className="text-xs text-neutral-400">
            Dzień {dayIndex + 1} z 7
          </span>
          <button type="button" aria-label="Następny dzień" className={dayArrowClass} onClick={() => stepDay(1)}>
            ›
          </button>
        </div>
      )}

      {hasData && (
        <>
          <WeekGrid
            members={players}
            allMembers={members}
            todayIso={todayIso}
            eventsByDate={eventsByDate}
            {...rowProps}
          />
          <RosterFooter coaches={coaches} weekDates={weekDates} {...rowProps} />
        </>
      )}

      {cell && myRow && (
        <CellPopover anchor={cell.rect} onClose={closeCell}>
          <DayAvailabilityEditor key={cell.date} date={cell.date} entry={entryFor(myRow, cell.date)} onClose={closeCell} />
        </CellPopover>
      )}

      {isBulkOpen && myRow && (
        <BulkAvailabilityDialog
          member={myRow}
          dates={weekDates}
          eligible={eligibleBulkDays(myRow, weekDates, todayIso)}
          onClose={() => setIsBulkOpen(false)}
        />
      )}
    </section>
  )
}
