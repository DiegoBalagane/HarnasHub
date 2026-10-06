import { memo, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import type { OpponentSummary } from '../../../services/opponentsApi'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { useOpponents, useOpponentsWithHidden } from '../hooks/useOpponents'
import { opponentProfilePath } from '../paths'
import { OpponentActionsMenu } from './OpponentActionsMenu'
import { RecordBadge } from './RecordBadge'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })
const dateTimeFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })

/** Polish plural of "notatka" — 1 notatka, 2–4 notatki (but 12–14 notatek), otherwise notatek. */
function noteNoun(count: number): string {
  if (count === 1) return 'notatka'
  const lastDigit = count % 10
  const lastTwo = count % 100
  return lastDigit >= 2 && lastDigit <= 4 && (lastTwo < 12 || lastTwo > 14) ? 'notatki' : 'notatek'
}

/** Every known opponent as a card linking to their profile, with a name filter; upcoming opponents come first. */
export function OpponentList() {
  const [filter, setFilter] = useState('')
  const [showHidden, setShowHidden] = useState(false)
  const canManage = useIsCoachOrManager()
  const visibleQuery = useOpponents()
  const withHiddenQuery = useOpponentsWithHidden(showHidden)
  const { data: opponents, isLoading, isError } = showHidden ? withHiddenQuery : visibleQuery

  const visible = useMemo(() => {
    const needle = filter.trim().toLowerCase()
    return needle ? opponents?.filter((o) => o.name.toLowerCase().includes(needle)) : opponents
  }, [opponents, filter])

  return (
    <div className="flex w-full flex-col gap-4">
      <input
        placeholder="Szukaj przeciwnika…"
        value={filter}
        onChange={(event) => setFilter(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {canManage && (
        <label className="flex items-center gap-2 self-start text-sm text-neutral-400">
          <input
            type="checkbox"
            checked={showHidden}
            onChange={(event) => setShowHidden(event.target.checked)}
          />
          Pokaż ukrytych
        </label>
      )}

      {isLoading && <p className="text-neutral-400">Ładowanie przeciwników…</p>}
      {isError && <p className="text-danger-400">Nie udało się pobrać przeciwników.</p>}
      {opponents?.length === 0 && (
        <p className="text-neutral-400">
          Brak przeciwników — pojawią się tu automatycznie z notatek, wyników i wydarzeń z wpisanym
          przeciwnikiem.
        </p>
      )}
      {opponents && opponents.length > 0 && visible?.length === 0 && (
        <p className="text-neutral-400">Nikt nie pasuje do „{filter}”.</p>
      )}

      <ul className="grid gap-3 sm:grid-cols-2">
        {visible?.map((opponent) => (
          <OpponentCard key={opponent.name} opponent={opponent} canManage={canManage} />
        ))}
      </ul>
    </div>
  )
}

/** One opponent tile: name, record, note count and the next/last game. */
const OpponentCard = memo(function OpponentCard({
  opponent,
  canManage,
}: {
  opponent: OpponentSummary
  canManage: boolean
}) {
  return (
    <li className="relative">
      <Link
        to={opponentProfilePath(opponent.name)}
        className={`flex h-full flex-col gap-1 rounded-md border border-neutral-800 p-4 transition hover:border-neutral-600 ${
          opponent.isHidden ? 'opacity-60' : ''
        } ${canManage ? 'pr-12' : ''}`}
      >
        <div className="flex items-center justify-between gap-2">
          <p className="truncate font-medium">
            {opponent.name}
            {opponent.isHidden && <span className="ml-2 text-xs font-normal text-neutral-500">ukryty</span>}
          </p>
          <RecordBadge wins={opponent.wins} losses={opponent.losses} draws={opponent.draws} />
        </div>
        {opponent.nextEventAtUtc && (
          <p className="text-sm text-warning-400">
            Najbliższy mecz: {dateTimeFormatter.format(new Date(opponent.nextEventAtUtc))}
          </p>
        )}
        <p className="text-xs text-neutral-500">
          {opponent.noteCount} {noteNoun(opponent.noteCount)}
          {opponent.lastPlayedAtUtc &&
            ` · ostatnio grane ${dateFormatter.format(new Date(opponent.lastPlayedAtUtc))}`}
        </p>
      </Link>
      {canManage && (
        <OpponentActionsMenu
          name={opponent.name}
          isHidden={opponent.isHidden}
          className="absolute right-2 top-2"
        />
      )}
    </li>
  )
})
