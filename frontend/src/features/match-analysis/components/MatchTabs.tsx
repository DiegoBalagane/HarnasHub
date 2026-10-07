import { useState, type ReactNode } from 'react'
import { Tabs, type TabItem } from '../../../components/ui/Tabs'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { isMissingTimeline, useMatchTimeline } from '../hooks/useMatchAnalysis'
import { AttachDemoButton } from './AttachDemoButton'
import { ExcludedPlayersBar } from './ExcludedPlayersBar'
import { MatchDeepAnalysisPanel } from './MatchDeepAnalysisPanel'
import { MatchInsightsPanel } from './MatchInsightsPanel'
import { MatchPlayerAnalysis } from './MatchPlayerAnalysis'
import { RoundTimeline } from './RoundTimeline'

type MatchTab = 'overview' | 'rounds' | 'players'

const matchTabs: readonly TabItem<MatchTab>[] = [
  { id: 'overview', label: 'Przegląd' },
  { id: 'rounds', label: 'Rundy' },
  { id: 'players', label: 'Gracze' },
]

interface MatchTabsProps {
  matchResultId: string
  /** Rendered at the top of the overview tab (score context, notes…). */
  overview?: ReactNode
  /** The per-player stats panel shown on the players tab. */
  players: ReactNode
}

/** Match page tabs: Przegląd (insights), Rundy (round timeline) and Gracze (player stats), with "Dołącz demkę" when the match has no timeline. */
export function MatchTabs({ matchResultId, overview, players }: MatchTabsProps) {
  const [tab, setTab] = useState<MatchTab>('overview')
  const canManage = useIsCoachOrManager()
  const { data: timeline, isLoading, error } = useMatchTimeline(matchResultId)
  const missing = isMissingTimeline(error)

  const noTimeline = (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-800 p-3 text-sm text-neutral-400">
      <p>Ten mecz nie ma jeszcze osi czasu z demki — rundy, ekonomia i wnioski pojawią się po jej dołączeniu.</p>
      {canManage && <AttachDemoButton matchResultId={matchResultId} />}
    </div>
  )

  return (
    <div className="flex flex-col gap-4">
      <Tabs tabs={matchTabs} value={tab} onChange={setTab} />

      {tab === 'overview' && (
        <div className="flex flex-col gap-4">
          {overview}
          {missing ? noTimeline : <MatchInsightsPanel matchResultId={matchResultId} />}
          {!missing && <MatchDeepAnalysisPanel matchResultId={matchResultId} />}
        </div>
      )}

      {tab === 'rounds' && (
        <>
          {isLoading && <p className="text-sm text-neutral-400">Ładowanie osi czasu…</p>}
          {missing && noTimeline}
          {error && !missing && <p className="text-sm text-danger-400">{error.message}</p>}
          {timeline && <RoundTimeline matchResultId={matchResultId} timeline={timeline} />}
          {timeline && canManage && <AttachDemoButton matchResultId={matchResultId} label="Podmień demkę" />}
        </>
      )}

      {tab === 'players' && (
        <div className="flex flex-col gap-6">
          {players}
          {!missing && <ExcludedPlayersBar matchResultId={matchResultId} />}
          {!missing && <MatchPlayerAnalysis matchResultId={matchResultId} />}
        </div>
      )}
    </div>
  )
}
