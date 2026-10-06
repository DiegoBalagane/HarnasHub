import { memo, useState } from 'react'
import { Tabs, type TabItem } from '../../../components/ui/Tabs'
import type { IndividualForm } from '../../../services/opponentReportApi'
import { PlayerFormCard } from './PlayerFormCard'

type Side = 'theirs' | 'ours'

const tabs: readonly TabItem<Side>[] = [
  { id: 'theirs', label: 'Rywal' },
  { id: 'ours', label: 'My' },
]

/** "Forma indywidualna": per-player cards of the opponent or of our players, built from team and solo FACEIT games. */
export const IndividualFormSection = memo(function IndividualFormSection({ form }: { form: IndividualForm | null | undefined }) {
  const [side, setSide] = useState<Side>('theirs')

  if (!form || (form.theirs.players.length === 0 && form.ours.players.length === 0)) {
    return null
  }

  const players = form[side].players

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
    </section>
  )
})
