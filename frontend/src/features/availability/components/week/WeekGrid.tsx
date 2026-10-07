import { useMemo } from 'react'
import type { MemberWeek } from '../../../../services/availabilityApi'
import type { CalendarEvent } from '../../../../services/calendarApi'
import { computeDaySummary } from '../../daySummary'
import { sectionOf } from '../../rosterSections'
import { entryFor } from '../../weekDates'
import { gridColumns } from '../../weekModel'
import { DayHeader } from './DayHeader'
import { MemberRow, type RowShared } from './MemberRow'

interface WeekGridProps extends RowShared {
  /** Players shown as rows (coaches live in the footer). */
  members: MemberWeek[]
  /** Everyone, used for header counts. */
  allMembers: MemberWeek[]
  todayIso: string
  eventsByDate: Map<string, CalendarEvent[]>
}

/** Day headers plus one slim row per player. Rows share the available height (`minmax(26px,1fr)`), so up to ~12 players fit without scrolling; only beyond that the wrapper scrolls. */
export function WeekGrid({ members, allMembers, todayIso, eventsByDate, ...rowProps }: WeekGridProps) {
  const { dates } = rowProps
  const headers = useMemo(
    () =>
      dates.map((date) => ({
        date,
        summary: computeDaySummary(allMembers, date),
        absentNames: allMembers
          .filter((member) => !member.hiddenFromCalendar && entryFor(member, date)?.status === 'Off')
          .map((member) => member.inGameNickname ?? member.displayName),
      })),
    [allMembers, dates],
  )

  return (
    <div className="min-h-0 flex-1 overflow-y-auto">
      <div
        className="grid min-h-full gap-x-1"
        style={{
          gridTemplateColumns: gridColumns(dates.length),
          gridTemplateRows: `auto repeat(${members.length}, minmax(26px, 1fr))`,
        }}
      >
        <div />
        {headers.map(({ date, summary, absentNames }) => (
          <DayHeader
            key={date}
            date={date}
            isToday={date === todayIso}
            summary={summary}
            events={eventsByDate.get(date) ?? []}
            absentNames={absentNames}
          />
        ))}
        {members.map((member, index) => (
          <MemberRow
            key={member.userId}
            member={member}
            divider={index > 0 && sectionOf(member) !== sectionOf(members[index - 1])}
            {...rowProps}
          />
        ))}
      </div>
    </div>
  )
}
