import { Fragment, useMemo, useState } from 'react'
import type { CalendarEvent } from '../../../services/calendarApi'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useUpcomingEvents } from '../../calendar/hooks/useCalendar'
import { computeDaySummary } from '../daySummary'
import { useWeekAvailability } from '../hooks/useAvailability'
import { weekdayLabels } from '../labels'
import {
  addDaysIso,
  buildWeekDates,
  entryFor,
  getWeekStartIso,
  parseIsoDate,
  rosterSlotRank,
  toInitials,
  toIsoDate,
} from '../weekDates'
import { DayAvailabilityEditor } from './DayAvailabilityEditor'
import { DayStatusBadge } from './DayStatusBadge'

const dayNumberFormatter = new Intl.DateTimeFormat('pl-PL', { day: '2-digit', month: '2-digit' })
const navButtonClass =
  'rounded-md border border-neutral-700 px-3 py-1 text-xs text-neutral-300 transition hover:border-neutral-500'

/** Weekly availability grid: one row per team member, one column per day, own cells are editable. */
export function WeeklyCalendar() {
  const currentUserId = useAuthStore((state) => state.userId)
  const currentWeekStart = useMemo(() => getWeekStartIso(new Date()), [])
  const [activeTab, setActiveTab] = useState<'upcoming' | 'history'>('upcoming')
  const [weekStart, setWeekStart] = useState(currentWeekStart)
  const [editingDate, setEditingDate] = useState<string | null>(null)
  const { data, isLoading, isError } = useWeekAvailability(weekStart)
  const { data: events } = useUpcomingEvents()

  const weekDates = useMemo(() => buildWeekDates(weekStart), [weekStart])
  const todayIso = toIsoDate(new Date())
  const isHistory = activeTab === 'history'

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
        // Main squad above the bench above anyone unassigned, matching the backend's own ordering.
        const slotRank = rosterSlotRank(left.rosterSlot) - rosterSlotRank(right.rosterSlot)

        if (slotRank !== 0) {
          return slotRank
        }

        return (left.inGameNickname ?? left.displayName).localeCompare(
          right.inGameNickname ?? right.displayName,
          'pl',
        )
      }),
    [data],
  )

  const myRow = members.find((member) => member.userId === currentUserId)

  function goToWeek(nextWeekStart: string) {
    setWeekStart(nextWeekStart)
    setEditingDate(null)
  }

  function switchTab(tab: 'upcoming' | 'history') {
    setActiveTab(tab)
    setEditingDate(null)
    setWeekStart(tab === 'history' ? addDaysIso(currentWeekStart, -7) : currentWeekStart)
  }

  // Upcoming never goes earlier than the current week; history never reaches into it — past days
  // live exclusively in the History tab, matching the backend's own not-in-the-past edit rule.
  const canGoBack = isHistory || weekStart > currentWeekStart
  const canGoForward = !isHistory || addDaysIso(weekStart, 7) < currentWeekStart

  return (
    <section className="flex w-full flex-col gap-3 rounded-md border border-neutral-800 p-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-3">
          <h2 className="font-medium">Dostępność w tygodniu</h2>
          <div className="flex gap-1 rounded-md border border-neutral-800 p-0.5 text-xs">
            <button
              type="button"
              onClick={() => switchTab('upcoming')}
              className={`rounded px-2 py-1 transition ${
                !isHistory ? 'bg-neutral-800 text-neutral-100' : 'text-neutral-500 hover:text-neutral-300'
              }`}
            >
              Nadchodzące
            </button>
            <button
              type="button"
              onClick={() => switchTab('history')}
              className={`rounded px-2 py-1 transition ${
                isHistory ? 'bg-neutral-800 text-neutral-100' : 'text-neutral-500 hover:text-neutral-300'
              }`}
            >
              Historia
            </button>
          </div>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            disabled={!canGoBack}
            className={`${navButtonClass} disabled:cursor-not-allowed disabled:opacity-30`}
            onClick={() => goToWeek(addDaysIso(weekStart, -7))}
          >
            ◀ Poprzedni tydzień
          </button>
          {!isHistory && (
            <button type="button" className={navButtonClass} onClick={() => goToWeek(currentWeekStart)}>
              Dziś
            </button>
          )}
          <button
            type="button"
            disabled={!canGoForward}
            className={`${navButtonClass} disabled:cursor-not-allowed disabled:opacity-30`}
            onClick={() => goToWeek(addDaysIso(weekStart, 7))}
          >
            Następny tydzień ▶
          </button>
        </div>
      </div>

      {isHistory && (
        <p className="text-xs text-neutral-500">
          Historia jest tylko do odczytu — edycja dostępności działa wyłącznie dla dzisiaj i kolejnych dni, w
          zakładce „Nadchodzące”.
        </p>
      )}

      {isLoading && <p className="text-neutral-400">Ładowanie dostępności…</p>}
      {isError && <p className="text-red-400">Nie udało się pobrać dostępności.</p>}

      {!isLoading && !isError && members.length === 0 && (
        <p className="text-neutral-400">Brak członków drużyny do wyświetlenia.</p>
      )}

      {!isLoading && !isError && members.length > 0 && (
        <div className="overflow-x-auto">
          <div className="grid min-w-[900px] grid-cols-[minmax(150px,180px)_repeat(7,minmax(0,1fr))] gap-1">
            <div />
            {weekDates.map((date, index) => {
              const summary = computeDaySummary(members, date)

              return (
                <div
                  key={date}
                  className={`rounded-md border px-2 py-1 text-center ${
                    date === todayIso ? 'border-blue-500' : 'border-neutral-800'
                  }`}
                >
                  <p className="text-xs font-medium text-neutral-200">{weekdayLabels[index]}</p>
                  <p className="text-[11px] text-neutral-500">
                    {dayNumberFormatter.format(parseIsoDate(date))}
                  </p>
                  <p
                    title="Ilu członków składu zadeklarowało dostępność tego dnia"
                    className={`mt-1 text-[11px] font-medium ${
                      summary.availableCount === summary.totalCount ? 'text-green-400' : 'text-neutral-400'
                    }`}
                  >
                    {summary.availableCount}/{summary.totalCount} dostępnych
                  </p>
                  {summary.commonWindow && (
                    <p className="text-[10px] text-neutral-500">
                      {summary.commonWindow.from}–{summary.commonWindow.to}
                    </p>
                  )}
                  {(eventsByDate.get(date) ?? []).map((event) => (
                    <span key={event.id} className="mt-1 block truncate text-[10px] text-red-400">
                      ● {event.title}
                    </span>
                  ))}
                </div>
              )
            })}

            {members.map((member) => {
              const isMyRow = member.userId === currentUserId

              return (
                <Fragment key={member.userId}>
                  <div
                    className={`flex items-center gap-2 truncate rounded-l-md px-2 py-1 text-sm ${
                      isMyRow ? 'bg-neutral-900' : ''
                    }`}
                  >
                    <span className="truncate text-neutral-200">
                      {member.inGameNickname ?? member.displayName}
                    </span>
                    {isMyRow && <span className="text-[10px] text-neutral-500">(Ty)</span>}
                  </div>
                  {weekDates.map((date, index) => {
                    const entry = entryFor(member, date)
                    const isLastColumn = index === weekDates.length - 1
                    const rowBackground = isMyRow ? 'bg-neutral-900' : ''
                    const roundedEnd = isLastColumn ? 'rounded-r-md' : ''

                    if (!entry) {
                      return <div key={date} className={`${rowBackground} ${roundedEnd}`} />
                    }

                    const isEditable = isMyRow && !isHistory && date >= todayIso

                    return isEditable ? (
                      <button
                        key={date}
                        type="button"
                        title="Kliknij, aby ustawić swoją dostępność"
                        onClick={() => setEditingDate(editingDate === date ? null : date)}
                        className={`p-0.5 transition hover:opacity-80 ${rowBackground} ${roundedEnd} ${
                          editingDate === date ? 'ring-1 ring-neutral-300' : ''
                        }`}
                      >
                        <DayStatusBadge entry={entry} />
                      </button>
                    ) : (
                      <div key={date} className={`p-0.5 ${isMyRow ? `${rowBackground} ${roundedEnd}` : ''}`}>
                        <DayStatusBadge entry={entry} />
                      </div>
                    )
                  })}
                </Fragment>
              )
            })}

            <div className="mt-2 border-t border-neutral-800 px-2 pt-2 text-xs text-neutral-500">Nie gra</div>
            {weekDates.map((date) => {
              const absentMembers = members.filter((member) => entryFor(member, date)?.status === 'Off')

              return (
                <div key={date} className="mt-2 flex flex-wrap gap-1 border-t border-neutral-800 pt-2">
                  {absentMembers.map((member) => (
                    <span
                      key={member.userId}
                      title={member.inGameNickname ?? member.displayName}
                      className="rounded-full border border-neutral-700 bg-neutral-900 px-2 py-0.5 text-[10px] text-neutral-400"
                    >
                      {toInitials(member.inGameNickname ?? member.displayName)}
                    </span>
                  ))}
                </div>
              )
            })}
          </div>
        </div>
      )}

      {editingDate !== null && myRow && (
        <DayAvailabilityEditor
          key={editingDate}
          date={editingDate}
          entry={entryFor(myRow, editingDate)}
          onClose={() => setEditingDate(null)}
        />
      )}
    </section>
  )
}
