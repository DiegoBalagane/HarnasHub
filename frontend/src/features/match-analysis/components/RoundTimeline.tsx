import { memo, useCallback, useMemo, useState } from 'react'
import type { MatchTimeline } from '../../../services/matchAnalysisApi'
import type { RoundTacticMatch } from '../../../services/tacticMatchingApi'
import { useMatchTacticMatches } from '../../tactics/hooks/useTacticMatching'
import { buyTypeLabels, buyTypeShort } from '../labels'
import { RoundReplayModal } from './RoundReplayModal'
import { RoundRow } from './RoundRow'

interface RoundTimelineProps {
  matchResultId: string
  timeline: MatchTimeline
}

/** Round-by-round match timeline: one expandable row per round with result, sides, buys, end reason, plant site and the
 * automatically matched Playbook tactic; an expanded round can be replayed in 2D. */
export const RoundTimeline = memo(function RoundTimeline({ matchResultId, timeline }: RoundTimelineProps) {
  const [expandedRound, setExpandedRound] = useState<number | null>(null)
  const [replayRound, setReplayRound] = useState<number | null>(null)
  const { data: tacticMatches } = useMatchTacticMatches(matchResultId)
  const matchesByRound = useMemo(
    () =>
      new Map<number, RoundTacticMatch>(
        (tacticMatches?.rounds ?? []).map((match) => [match.roundNumber, match]),
      ),
    [tacticMatches],
  )
  const toggle = useCallback((roundNumber: number) => {
    setExpandedRound((current) => (current === roundNumber ? null : roundNumber))
  }, [])
  const closeReplay = useCallback(() => setReplayRound(null), [])

  return (
    <section className="flex flex-col gap-3">
      {!timeline.ourTeamResolved && (
        <p className="rounded-md border border-warning-900/60 p-2 text-sm text-warning-300">
          Nie rozpoznano naszej drużyny — „my” to drużyna, która zaczęła po stronie T.
        </p>
      )}
      {tacticMatches && !tacticMatches.mapCalibrated && (
        <p className="text-xs text-warning-300">
          Mapa niekalibrowana — pozycje niedostępne: odtwarzanie 2D pokaże tylko zabójstwa, a rundy nie są
          dopasowywane do taktyk.
        </p>
      )}
      {tacticMatches?.mapCalibrated && !tacticMatches.hasPositions && (
        <p className="text-xs text-neutral-400">
          Demka przeanalizowana bez pozycji graczy — podmień ją, aby odtwarzać rundy 2D i dopasowywać taktyki
          po pozycjach.
        </p>
      )}
      <p className="text-xs text-neutral-500">
        Kupno (my vs rywal):{' '}
        {(Object.keys(buyTypeShort) as (keyof typeof buyTypeShort)[])
          .map((buyType) => `${buyTypeShort[buyType]} = ${buyTypeLabels[buyType]}`)
          .join(' · ')}
      </p>
      <ul className="flex flex-col gap-1">
        {timeline.rounds.map((round) => (
          <RoundRow
            key={round.number}
            round={round}
            expanded={expandedRound === round.number}
            onToggle={toggle}
            onReplay={setReplayRound}
            tacticMatch={matchesByRound.get(round.number)}
          />
        ))}
      </ul>
      {replayRound !== null && (
        <RoundReplayModal
          source="Match"
          sourceId={matchResultId}
          initialRound={replayRound}
          onClose={closeReplay}
        />
      )}
    </section>
  )
})
