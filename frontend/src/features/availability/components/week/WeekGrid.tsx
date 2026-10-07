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

/** Day headers plus one slim row per player (28–36 px, never stretched); a dozen players fit without scrolling and only beyond that the wrapper scrolls. */
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
    <div className="min-h-0 flex-1 overflow-y-auto px-0.5 pt-1">
      <div
        className="grid content-start gap-x-1 gap-y-1.5"
        style={{
          gridTemplateColumns: gridColumns(dates.length),
          // Rows keep a fixed comfortable height instead of stretching over the free space (big gaps with few players).
          gridTemplateRows: `auto repeat(${members.length}, minmax(28px, 36px))`,
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
