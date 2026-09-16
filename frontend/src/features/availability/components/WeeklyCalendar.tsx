import { Fragment, useMemo, useState } from 'react'
import type { DayEntry, MemberWeek } from '../../../services/availabilityApi'
import type { CalendarEvent } from '../../../services/calendarApi'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useUpcomingEvents } from '../../calendar/hooks/useCalendar'
import { useWeekAvailability } from '../hooks/useAvailability'
import { weekdayLabels } from '../labels'
import {
  addDaysIso,
  buildWeekDates,
  getWeekStartIso,
  parseIsoDate,
  toInitials,
  toIsoDate,
} from '../weekDates'
import { DayAvailabilityEditor } from './DayAvailabilityEditor'
import { DayStatusBadge } from './DayStatusBadge'

const dayNumberFormatter = new Intl.DateTimeFormat('pl-PL', { day: '2-digit', month: '2-digit' })
const navButtonClass =
  'rounded-md border border-neutral-700 px-3 py-1 text-xs text-neutral-300 transition hover:border-neutral-500'

/** Returns the member's entry for one day, or undefined when the server row is incomplete. */
function entryFor(member: MemberWeek, date: string): DayEntry | undefined {
  return member.days.find((day) => day.date === date)
}

/** Weekly availability grid: one row per team member, one column per day, own cells are editable. */
export function WeeklyCalendar() {
  const currentUserId = useAuthStore((state) => state.userId)
  const [weekStart, setWeekStart] = useState(() => getWeekStartIso(new Date()))
  const [editingDate, setEditingDate] = useState<string | null>(null)
  const { data, isLoading, isError } = useWeekAvailability(weekStart)
  const { data: events } = useUpcomingEvents()

  const weekDates = useMemo(() => buildWeekDates(weekStart), [weekStart])
  const todayIso = toIsoDate(new Date())

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
      [...(data?.members ?? [])].sort((left, right) =>
        left.displayName.localeCompare(right.displayName, 'pl'),
      ),
    [data],
  )

  const myRow = members.find((member) => member.userId === currentUserId)

  function goToWeek(nextWeekStart: string) {
    setWeekStart(nextWeekStart)
    setEditingDate(null)
  }

  return (
    <section className="flex w-full flex-col gap-3 rounded-md border border-neutral-800 p-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="font-medium">Dostępność w tygodniu</h2>
        <div className="flex gap-2">
          <button
            type="button"
            className={navButtonClass}
            onClick={() => goToWeek(addDaysIso(weekStart, -7))}
          >
            ◀ Poprzedni tydzień
          </button>
          <button
            type="button"
            className={navButtonClass}
            onClick={() => goToWeek(getWeekStartIso(new Date()))}
          >
            Dziś
          </button>
          <button type="button" className={navButtonClass} onClick={() => goToWeek(addDaysIso(weekStart, 7))}>
            Następny tydzień ▶
          </button>
        </div>
      </div>

      {isLoading && <p className="text-neutral-400">Ładowanie dostępności…</p>}
      {isError && <p className="text-red-400">Nie udało się pobrać dostępności.</p>}

      {!isLoading && !isError && members.length === 0 && (
        <p className="text-neutral-400">Brak członków drużyny do wyświetlenia.</p>
      )}

      {!isLoading && !isError && members.length > 0 && (
        <div className="overflow-x-auto">
          <div className="grid min-w-[900px] grid-cols-[minmax(150px,180px)_repeat(7,minmax(0,1fr))] gap-1">
            <div />
            {weekDates.map((date, index) => (
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
                {(eventsByDate.get(date) ?? []).map((event) => (
                  <span key={event.id} className="mt-1 block truncate text-[10px] text-red-400">
                    ● {event.title}
                  </span>
                ))}
              </div>
            ))}

            {members.map((member) => (
              <Fragment key={member.userId}>
                <div className="flex items-center gap-2 truncate px-2 py-1 text-sm">
                  <span className="truncate text-neutral-200">{member.displayName}</span>
                  {member.userId === currentUserId && (
                    <span className="text-[10px] text-neutral-500">(Ty)</span>
                  )}
                </div>
                {weekDates.map((date) => {
                  const entry = entryFor(member, date)

                  if (!entry) {
                    return <div key={date} />
                  }

                  const isMine = member.userId === currentUserId

                  return isMine ? (
                    <button
                      key={date}
                      type="button"
                      title="Kliknij, aby ustawić swoją dostępność"
                      onClick={() => setEditingDate(editingDate === date ? null : date)}
                      className={`rounded-md p-0.5 transition hover:opacity-80 ${
                        editingDate === date ? 'ring-1 ring-neutral-300' : ''
                      }`}
                    >
                      <DayStatusBadge entry={entry} />
                    </button>
                  ) : (
                    <div key={date} className="p-0.5">
                      <DayStatusBadge entry={entry} />
                    </div>
                  )
                })}
              </Fragment>
            ))}

            <div className="mt-2 border-t border-neutral-800 px-2 pt-2 text-xs text-neutral-500">Nie gra</div>
            {weekDates.map((date) => {
              const absentMembers = members.filter((member) => entryFor(member, date)?.status === 'Off')

              return (
                <div key={date} className="mt-2 flex flex-wrap gap-1 border-t border-neutral-800 pt-2">
                  {absentMembers.map((member) => (
                    <span
                      key={member.userId}
                      title={member.displayName}
                      className="rounded-full border border-neutral-700 bg-neutral-900 px-2 py-0.5 text-[10px] text-neutral-400"
                    >
                      {toInitials(member.displayName)}
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
