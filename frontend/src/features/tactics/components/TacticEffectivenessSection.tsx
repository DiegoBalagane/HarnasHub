import { memo } from 'react'
import type { MapName } from '../../../services/nadesApi'
import { mapSideLabels } from '../../map-strategy/labels'
import { useTacticEffectiveness } from '../hooks/useTacticMatching'
import { UNCALIBRATED_MAP_MESSAGE } from '../labels'

interface TacticEffectivenessSectionProps {
  mapName: MapName
}

const percent = (part: number, total: number) => (total === 0 ? '—' : `${Math.round((100 * part) / total)}%`)

/** Playbook "Statystyki": how often each tactic of the map was recognised in our analysed rounds and how often it won. */
export const TacticEffectivenessSection = memo(function TacticEffectivenessSection({
  mapName,
}: TacticEffectivenessSectionProps) {
  const { data, isLoading, error } = useTacticEffectiveness(mapName)

  return (
    <div className="rounded-md border border-neutral-800 p-3">
      <h3 className="text-sm font-medium">Skuteczność taktyk (automatyczne dopasowanie rund)</h3>
      {isLoading && <p className="mt-1 text-sm text-neutral-400">Dopasowywanie rund do taktyk…</p>}
      {error && <p className="mt-1 text-sm text-danger-400">{error.message}</p>}
      {data && !data.mapCalibrated && (
        <p className="mt-1 text-sm text-warning-300">{UNCALIBRATED_MAP_MESSAGE}</p>
      )}
      {data?.mapCalibrated && (
        <>
          <p className="mt-1 text-xs text-neutral-500">
            {data.matchesAnalyzed} meczów, {data.roundsMatched}/{data.roundsAnalyzed} rund dopasowanych
            {data.matchesSkipped > 0 ? ` · pominięto ${data.matchesSkipped}` : ''} · pozycje z pierwszych 25
            s, granaty z 40 s
          </p>
          {data.tactics.length === 0 ? (
            <p className="mt-1 text-sm text-neutral-500">Brak taktyk na tej mapie.</p>
          ) : (
            <ul className="mt-2 flex flex-col gap-1 text-sm text-neutral-300">
              {data.tactics.map((tactic) => (
                <li key={tactic.tacticId} className="flex justify-between gap-3">
                  <span>
                    {tactic.name} <span className="text-neutral-500">· {mapSideLabels[tactic.side]}</span>
                  </span>
                  <span>
                    {tactic.roundsWon}/{tactic.roundsPlayed} rund (
                    {percent(tactic.roundsWon, tactic.roundsPlayed)}) · {tactic.matches} meczów
                  </span>
                </li>
              ))}
            </ul>
          )}
        </>
      )}
    </div>
  )
})
