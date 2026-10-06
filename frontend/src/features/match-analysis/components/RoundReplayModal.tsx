import { useState } from 'react'
import { Modal } from '../../../components/Modal'
import type { ReplaySource } from '../../../services/replayApi'
import { RoundReplay } from './RoundReplay'

interface RoundReplayModalProps {
  source: ReplaySource
  /** Match result id or opponent demo id, depending on `source`. */
  sourceId: string
  initialRound: number
  /** Context shown in the title, e.g. the opponent or map name. */
  label?: string
  onClose: () => void
}

/** The round replay in a wide modal with previous/next round navigation. */
export function RoundReplayModal({ source, sourceId, initialRound, label, onClose }: RoundReplayModalProps) {
  const [roundNumber, setRoundNumber] = useState(initialRound)

  return (
    <Modal title={`Odtwarzanie 2D${label ? ` — ${label}` : ''}`} onClose={onClose} extraWide>
      <RoundReplay
        source={source}
        sourceId={sourceId}
        roundNumber={roundNumber}
        onRoundChange={setRoundNumber}
      />
    </Modal>
  )
}
