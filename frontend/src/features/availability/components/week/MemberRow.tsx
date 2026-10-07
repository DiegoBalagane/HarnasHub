import React from 'react'
import type { MemberWeek } from '../../../../services/availabilityApi'
import { entryFor } from '../../weekDates'
import { AvailabilityPill } from './AvailabilityPill'

/** Props shared by every row renderer (grid and coach footer). */
export interface RowShared {
  dates: string[]
  currentUserId: string | null
  canEditDate: (date: string) => boolean
  activeDate: string | null
  onCellOpen: (date: string, rect: DOMRect) => void
  /** Opens the bulk "set for the week" dialog; the button shows only on the own row and only when given. */
  onBulk?: () => void
}

interface MemberRowProps extends RowShared {
  member: MemberWeek
  /** Draws a thin divider above the row (section change). */
  divider: boolean
}

/** One player: name cell followed by one availability pill per visible day (cells flow into the parent grid, which sizes the rows). */
export const MemberRow = React.memo(function MemberRow({
  member,
  divider,
  dates,
  currentUserId,
  canEditDate,
  activeDate,
  onCellOpen,
  onBulk,
}: MemberRowProps) {
  const mine = member.userId === currentUserId
  const name = member.inGameNickname ?? member.displayName
  const rowTone = mine ? 'bg-primary-500/5' : ''
  const border = divider ? 'border-t border-neutral-800' : ''

  return (
    <>
      <div
        className={`flex items-center gap-1.5 truncate rounded-l-md px-2 text-sm ${rowTone} ${border} ${
          mine ? 'border-l-2 border-l-primary-500' : ''
        }`}
      >
        <span className={`truncate ${mine ? 'font-semibold text-white' : 'text-neutral-300'}`}>{name}</span>
        {mine && <span className="text-[10px] text-primary-400">(Ty)</span>}
        {member.hiddenFromCalendar && (
          <span className="text-[10px] text-neutral-500" title="Manager ukrył Cię w kalendarzu — widzisz tylko Ty">
            (ukryty)
          </span>
        )}
        {mine && onBulk && (
          <button
            type="button"
            aria-label="Ustaw dla tygodnia…"
            title="Ustaw dla tygodnia…"
            onClick={onBulk}
            className="ml-auto rounded px-1 text-xs text-neutral-400 hover:bg-neutral-800 hover:text-primary-300"
          >
            ✎
          </button>
        )}
      </div>
      {dates.map((date) => (
        <div key={date} className={`flex items-center px-0.5 py-0.5 ${rowTone} ${border}`}>
          <AvailabilityPill
            entry={entryFor(member, date)}
            name={`${name} ${date}`}
            mine={mine}
            editable={mine && canEditDate(date)}
            active={mine && activeDate === date}
            onOpen={(rect) => onCellOpen(date, rect)}
          />
        </div>
      ))}
    </>
  )
})
