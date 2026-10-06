import { memo, useMemo } from 'react'
import type { FormGame } from '../../../services/opponentReportApi'
import { useOpponentDemos } from '../hooks/useOpponentDemos'
import { DemoUploadPanel } from './DemoUploadPanel'
import { FaceitMatchLinks } from './FaceitMatchLinks'
import { OpponentDemoList } from './OpponentDemoList'

interface OpponentDemosSectionProps {
  opponentName: string
  canManage: boolean
  /** Their latest FACEIT team matches, offered as room links for manual demo downloads. */
  recentGames?: FormGame[]
}

/** "Demki rywala": upload / FACEIT download (coach/manager) and the list of analysed demos behind the tendencies. */
export const OpponentDemosSection = memo(function OpponentDemosSection({
  opponentName,
  canManage,
  recentGames = [],
}: OpponentDemosSectionProps) {
  const { data, isLoading, isError } = useOpponentDemos(opponentName)
  const analysedMatchIds = useMemo(
    () => new Set((data?.demos ?? []).flatMap((demo) => (demo.faceitMatchId ? [demo.faceitMatchId] : []))),
    [data?.demos],
  )

  return (
    <section className="flex flex-col gap-3">
      <h2 className="text-lg font-semibold">Demki rywala</h2>
      {isLoading && <p className="text-sm text-neutral-400">Ładowanie demek…</p>}
      {isError && <p className="text-sm text-danger-400">Nie udało się pobrać listy demek.</p>}
      {data && (
        <>
          {canManage && data.storageConfigured && (
            <DemoUploadPanel opponentName={opponentName} autoDownloadAvailable={data.autoDownloadAvailable} />
          )}
          {canManage && !data.storageConfigured && (
            <p className="text-sm text-neutral-400">
              Magazyn plików nie jest skonfigurowany — wgrywanie demek jest niedostępne.
            </p>
          )}
          {canManage && data.storageConfigured && !data.autoDownloadAvailable && (
            <FaceitMatchLinks games={recentGames} analysedMatchIds={analysedMatchIds} />
          )}
          <OpponentDemoList opponentName={opponentName} demos={data.demos} canManage={canManage} />
        </>
      )}
    </section>
  )
})
