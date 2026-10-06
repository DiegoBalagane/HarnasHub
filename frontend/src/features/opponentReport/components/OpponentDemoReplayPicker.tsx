import { memo, useState } from 'react'
import type { OpponentDemo } from '../../../services/opponentDemosApi'
import { RoundReplayModal } from '../../match-analysis/components/RoundReplayModal'

interface OpponentDemoReplayPickerProps {
  demo: OpponentDemo
  opponentName: string
}

/** Round picker + "Odtwórz" for one analysed opponent demo; opens the 2D replay modal. */
export const OpponentDemoReplayPicker = memo(function OpponentDemoReplayPicker({
  demo,
  opponentName,
}: OpponentDemoReplayPickerProps) {
  const [round, setRound] = useState(1)
  const [open, setOpen] = useState(false)

  if (demo.roundsCount === 0) return null

  return (
    <span className="flex items-center gap-1">
      <select
        value={round}
        onChange={(event) => setRound(Number(event.target.value))}
        aria-label="Runda do odtworzenia"
        className="rounded border border-neutral-800 bg-neutral-950 px-1 py-0.5 text-xs"
      >
        {Array.from({ length: demo.roundsCount }, (_, index) => index + 1).map((number) => (
          <option key={number} value={number}>
            R{number}
          </option>
        ))}
      </select>
      <button type="button" onClick={() => setOpen(true)} className="text-xs text-info-300 hover:underline">
        Odtwórz
      </button>
      {open && (
        <RoundReplayModal
          source="OpponentDemo"
          sourceId={demo.id}
          initialRound={round}
          label={`${opponentName} · ${demo.mapName ?? demo.rawMapName ?? 'nieznana mapa'}`}
          onClose={() => setOpen(false)}
        />
      )}
    </span>
  )
})
