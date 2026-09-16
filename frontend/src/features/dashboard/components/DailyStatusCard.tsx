import React, { useMemo } from 'react'
import type { DailyTeamStatus, MemberDayStatus } from '../../../services/dashboardApi'
import { DayStatusBadge } from '../../availability/components/DayStatusBadge'
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
      <span className="truncate text-sm text-neutral-200">{member.displayName}</span>
      <span className="w-28 shrink-0">
        <DayStatusBadge entry={member} />
      </span>
    </li>
  )
})

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
        <div className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1">
          <p className="truncate text-sm font-medium">{day.event.title}</p>
          <p className="text-xs text-neutral-400">
            {eventTypeLabels[day.event.type]} · {timeFormatter.format(new Date(day.event.startsAtUtc))}
          </p>
        </div>
      ) : (
        <p className="text-xs text-neutral-500">Brak wydarzeń tego dnia</p>
      )}

      {playing.length === 0 ? (
        <p className="text-sm text-neutral-500">Nikt nie zgłosił dostępności.</p>
      ) : (
        <ul className="flex flex-col gap-1">
          {playing.map((member) => (
            <MemberRow key={member.userId} member={member} />
          ))}
        </ul>
      )}

      {absent.length > 0 && (
        <div className="border-t border-neutral-800 pt-2">
          <p className="mb-1 text-xs text-neutral-500">Nie gra</p>
          <ul className="flex flex-col gap-1 opacity-60">
            {absent.map((member) => (
              <MemberRow key={member.userId} member={member} />
            ))}
          </ul>
        </div>
      )}
    </section>
  )
})
