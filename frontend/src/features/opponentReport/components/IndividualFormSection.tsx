import { memo, useState } from 'react'
import { Link } from 'react-router-dom'
import { Tabs, type TabItem } from '../../../components/ui/Tabs'
import type { IndividualForm, UnresolvedRosterPlayer } from '../../../services/opponentReportApi'
import { PlayerFormCard } from './PlayerFormCard'

type Side = 'theirs' | 'ours'

const tabs: readonly TabItem<Side>[] = [
  { id: 'theirs', label: 'Rywal' },
  { id: 'ours', label: 'My' },
]

interface IndividualFormSectionProps {
  form: IndividualForm | null | undefined
  /** Roster players without a FACEIT account found, listed on the "My" tab with the reason. */
  unresolved?: UnresolvedRosterPlayer[] | null
  /** Managers get a link to the admin panel where the FACEIT nickname can be set. */
  canSetNickname?: boolean
}

/** "Forma indywidualna": per-player cards of the opponent or of our players, built from team and solo FACEIT games. */
export const IndividualFormSection = memo(function IndividualFormSection({
  form,
  unresolved,
  canSetNickname = false,
}: IndividualFormSectionProps) {
  const [side, setSide] = useState<Side>('theirs')
  const missing = unresolved ?? []
  const hasForm = !!form && (form.theirs.players.length > 0 || form.ours.players.length > 0)

  if (!hasForm && missing.length === 0) {
    return null
  }

  const players = form?.[side].players ?? []

  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-medium">Forma indywidualna</h2>
      <p className="text-xs text-neutral-500">
        Wszystkie mecze graczy z ostatnich 120 dni — drużynowe (≥ 3 graczy z listy w jednej drużynie) i solo. Strzałka: K/D z
        ostatnich 10 meczów vs wcześniejsze.
      </p>
      <Tabs tabs={tabs} value={side} onChange={setSide} />
      {players.length === 0 ? (
        <p className="text-sm text-neutral-400">Brak powiązanych graczy FACEIT.</p>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {players.map((player) => (
            <PlayerFormCard key={player.playerId} player={player} />
          ))}
        </div>
      )}
      {side === 'ours' && missing.length > 0 && <UnresolvedPlayers players={missing} canSetNickname={canSetNickname} />}
    </section>
  )
})

/** Roster players that could not be matched to a FACEIT account, each with the reason and (for Managers) a link to fix it. */
function UnresolvedPlayers({ players, canSetNickname }: { players: UnresolvedRosterPlayer[]; canSetNickname: boolean }) {
  return (
    <div className="flex flex-col gap-1 rounded-md border border-warning-500/40 bg-warning-500/5 p-3">
      <h3 className="text-sm font-medium">Nie znaleziono konta FACEIT</h3>
      <ul aria-label="Gracze bez konta FACEIT" className="flex flex-col gap-0.5 text-sm text-neutral-300">
        {players.map((player) => (
          <li key={player.userId}>
            <span className="font-medium">{player.displayName}</span> — {player.reason}
          </li>
        ))}
      </ul>
      {canSetNickname && (
        <Link to="/admin" className="text-xs text-primary-400 hover:underline">
          Ustaw nick FACEIT w panelu admina
        </Link>
      )}
    </div>
  )
}
