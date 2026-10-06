import { memo } from 'react'
import type { MapSide } from '../../../../services/mapStrategyApi'
import type { DemoNadeRound } from '../../../../services/tacticsApi'
import { mapSideLabels, mapSides } from '../../../map-strategy/labels'

interface DemoRoundPickerProps {
  rounds: DemoNadeRound[]
  selectedRound: number
  side: MapSide
  onRoundChange: (roundNumber: number) => void
  onSideChange: (side: MapSide) => void
}

/** Round list (score after the round, winner, grenade count) plus the T/CT toggle. */
export const DemoRoundPicker = memo(function DemoRoundPicker({
  rounds,
  selectedRound,
  side,
  onRoundChange,
  onSideChange,
}: DemoRoundPickerProps) {
  return (
    <div className="flex flex-col gap-2">
      <div className="flex overflow-hidden self-start rounded-md border border-neutral-800">
        {mapSides.map((option) => (
          <button
            key={option}
            type="button"
            onClick={() => onSideChange(option)}
            className={`px-3 py-1.5 text-sm ${side === option ? 'bg-neutral-100 text-neutral-900' : 'text-neutral-400 hover:text-white'}`}
          >
            {mapSideLabels[option]}
          </button>
        ))}
      </div>

      <ul className="flex max-h-72 flex-col gap-1 overflow-y-auto pr-1">
        {rounds.map((round) => {
          const sideNades = round.grenades.filter((grenade) => grenade.side === side).length
          return (
            <li key={round.number}>
              <button
                type="button"
                onClick={() => onRoundChange(round.number)}
                className={`flex w-full items-center justify-between gap-2 rounded-md border px-2 py-1.5 text-left text-xs ${
                  round.number === selectedRound
                    ? 'border-neutral-400 bg-neutral-800 text-white'
                    : 'border-neutral-800 text-neutral-300 hover:border-neutral-600'
                }`}
              >
                <span className="font-medium">Runda {round.number}</span>
                <span className="tabular-nums">
                  T {round.terroristScore}:{round.counterTerroristScore} CT
                </span>
                <span className={round.winnerSide === 'T' ? 'text-side-t' : 'text-side-ct'}>
                  {round.winnerSide ? `wygrali ${round.winnerSide}` : '—'}
                </span>
                <span className="text-neutral-500">{sideNades} gr.</span>
              </button>
            </li>
          )
        })}
      </ul>
    </div>
  )
})
