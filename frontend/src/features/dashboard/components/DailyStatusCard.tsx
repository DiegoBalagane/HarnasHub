import React, { useMemo } from 'react'
import { Link } from 'react-router-dom'
import type { DailyTeamStatus, MemberDayStatus } from '../../../services/dashboardApi'
import { DayStatusBadge } from '../../availability/components/DayStatusBadge'
import { compareSections, sectionOf } from '../../availability/rosterSections'
import { parseIsoDate } from '../../availability/weekDates'
import { eventTypeLabels } from '../../calendar/labels'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', {
  weekday: 'long',
  day: '2-digit',
  month: 'long',
})
const timeFormatter = new Intl.DateTimeFormat('pl-PL', { timeStyle: 'short' })

/** Members who are not marked off or on vacation — i.e. the ones who can play that day. */
function isPlaying(member: MemberDayStatus): boolean {
  return !member.isVacation && member.status !== 'Off'
}

/** One member row: display name on the left, effective status badge on the right. */
const MemberRow = React.memo(function MemberRow({ member }: { member: MemberDayStatus }) {
  return (
    <li className="flex items-center justify-between gap-2">
      <span className="truncate text-sm text-neutral-200">{member.inGameNickname ?? member.displayName}</span>
      <span className="w-28 shrink-0">
        <DayStatusBadge entry={member} />
      </span>
    </li>
  )
})

/** A member list grouped the same way as the weekly calendar: Main, then Bench, then Coach last in its own labelled group. */
function GroupedMemberList({ members }: { members: MemberDayStatus[] }) {
  const sorted = useMemo(() => [...members].sort(compareSections), [members])

  return (
    <ul className="flex flex-col gap-1">
      {sorted.map((member, index) => {
        const section = sectionOf(member)
        const previousSection = index > 0 ? sectionOf(sorted[index - 1]) : null

        return (
          <React.Fragment key={member.userId}>
            {section === 'Coach' && section !== previousSection && (
              <li className="mt-1 border-t border-neutral-800 pt-1 text-[10px] text-neutral-500">Trener</li>
            )}
            <MemberRow member={member} />
          </React.Fragment>
        )
      })}
    </ul>
  )
}

/** Dashboard tile listing a single day's event and who is available that day. */
export const DailyStatusCard = React.memo(function DailyStatusCard({
  title,
  day,
}: {
  title: string
  day: DailyTeamStatus
}) {
  const { playing, absent } = useMemo(
    () => ({
      playing: day.members.filter(isPlaying),
      absent: day.members.filter((member) => !isPlaying(member)),
    }),
    [day.members],
  )

  return (
    <section className="flex flex-col gap-2 rounded-md border border-neutral-800 p-4">
      <div className="flex items-baseline justify-between gap-2">
        <h2 className="font-medium">{title}</h2>
        <p className="truncate text-xs text-neutral-500">{dateFormatter.format(parseIsoDate(day.date))}</p>
      </div>

      {day.event ? (
        <Link
          to={`/calendar?event=${day.event.id}`}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 transition hover:border-neutral-600"
        >
          <p className="truncate text-sm font-medium">{day.event.title}</p>
          <p className="text-xs text-neutral-400">
            {eventTypeLabels[day.event.type]} · {timeFormatter.format(new Date(day.event.startsAtUtc))}
          </p>
        </Link>
      ) : (
        <p className="text-xs text-neutral-500">Brak wydarzeń tego dnia</p>
      )}

      {playing.length === 0 ? (
        <p className="text-sm text-neutral-500">Nikt nie zgłosił dostępności.</p>
      ) : (
        <GroupedMemberList members={playing} />
      )}

      {absent.length > 0 && (
        <div className="border-t border-neutral-800 pt-2 opacity-60">
          <p className="mb-1 text-xs text-neutral-500">Nie gra</p>
          <GroupedMemberList members={absent} />
        </div>
      )}
    </section>
  )
})
