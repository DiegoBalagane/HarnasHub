import { memo } from 'react'
import { useOpponentDemos } from '../hooks/useOpponentDemos'
import { DemoUploadPanel } from './DemoUploadPanel'
import { OpponentDemoList } from './OpponentDemoList'

interface OpponentDemosSectionProps {
  opponentName: string
  canManage: boolean
}

/** "Demki rywala": upload / FACEIT download (coach/manager) and the list of analysed demos behind the tendencies. */
export const OpponentDemosSection = memo(function OpponentDemosSection({
  opponentName,
  canManage,
}: OpponentDemosSectionProps) {
  const { data, isLoading, isError } = useOpponentDemos(opponentName)

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
          <OpponentDemoList opponentName={opponentName} demos={data.demos} canManage={canManage} />
        </>
      )}
    </section>
  )
})
