import { useState } from 'react'
import type { MemberWeek } from '../../../../services/availabilityApi'
import { gridColumns, summarizeWeek } from '../../weekModel'
import { MemberRow, type RowShared } from './MemberRow'

interface RosterFooterProps extends RowShared {
  coaches: MemberWeek[]
  /** The whole shown week, for the one-line summary (independent of the phone's single visible day). */
  weekDates: string[]
}

/** One slim line under the grid summarising coaches ("Trener: Capybar — off cały tydzień"); click expands their daily pills. */
export function RosterFooter({ coaches, weekDates, ...rowProps }: RosterFooterProps) {
  const [expanded, setExpanded] = useState(false)

  if (coaches.length === 0) return null

  const label = coaches.length > 1 ? 'Trenerzy' : 'Trener'
  const summary = coaches
    .map((coach) => `${coach.inGameNickname ?? coach.displayName} — ${summarizeWeek(coach, weekDates)}`)
    .join('; ')

  return (
    <div className="flex flex-col gap-1 border-t border-neutral-800 pt-1.5">
      <button
        type="button"
        aria-expanded={expanded}
        onClick={() => setExpanded((value) => !value)}
        className="flex items-center gap-1.5 truncate text-left text-xs text-neutral-400 hover:text-neutral-200"
      >
        <span aria-hidden>{expanded ? '▾' : '▸'}</span>
        <span className="truncate">
          {label}: {summary}
        </span>
      </button>
      {expanded && (
        <div
          className="grid gap-x-1"
          style={{ gridTemplateColumns: gridColumns(rowProps.dates.length), gridAutoRows: '28px' }}
        >
          {coaches.map((coach) => (
            <MemberRow key={coach.userId} member={coach} divider={false} {...rowProps} />
          ))}
        </div>
      )}
    </div>
  )
}
