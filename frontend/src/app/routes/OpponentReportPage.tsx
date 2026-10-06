import { useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { FaceitLinkForm } from '../../features/opponentReport/components/FaceitLinkForm'
import { InsightList } from '../../features/opponentReport/components/InsightList'
import { MapMatrix } from '../../features/opponentReport/components/MapMatrix'
import { MapTendenciesSection } from '../../features/opponentReport/components/MapTendenciesSection'
import { OpponentDemosSection } from '../../features/opponentReport/components/OpponentDemosSection'
import { PlayersToWatch, TeamFormSection } from '../../features/opponentReport/components/PlayersAndForm'
import { ReportToolbar } from '../../features/opponentReport/components/ReportToolbar'
import { VetoPlanSection } from '../../features/opponentReport/components/VetoPlanSection'
import { useOpponentReport } from '../../features/opponentReport/hooks/useOpponentReport'
import { opponentProfilePath } from '../../features/opponents/paths'
import type { VetoFormat } from '../../services/opponentReportApi'

/** Opponent report: FACEIT "them vs us" (TL;DR, map matrix, veto, players, form) plus tendencies from their demos. */
export function OpponentReportPage() {
  const [searchParams] = useSearchParams()
  const name = searchParams.get('name') ?? ''
  const canManage = useIsCoachOrManager()
  const [format, setFormat] = useState<VetoFormat>('Bo1')
  const { data: report, isLoading, isError } = useOpponentReport(name)

  return (
    <div className="flex w-full flex-col gap-6">
      <Link to={opponentProfilePath(name)} className="self-start text-sm text-neutral-400 hover:text-white">
        ← Profil przeciwnika
      </Link>

      {name.trim() === '' && <p className="text-neutral-400">Nie wybrano przeciwnika.</p>}
      {isLoading && <p className="text-neutral-400">Ładowanie raportu…</p>}
      {isError && <p className="text-danger-400">Nie udało się pobrać raportu przeciwnika.</p>}

      {report && (
        <>
          <header className="flex flex-col gap-1">
            <h1 className="text-2xl font-semibold">Raport: {report.opponentName}</h1>
            {report.link && (
              <p className="text-sm text-neutral-400">
                FACEIT: {report.link.players.map((player) => player.nickname).join(', ')}
              </p>
            )}
          </header>

          {canManage && report.faceitConfigured && (
            <FaceitLinkForm opponentName={report.opponentName} link={report.link} />
          )}
          {!report.link && !canManage && (
            <p className="text-neutral-400">Przeciwnik nie jest jeszcze powiązany z FACEIT — poproś trenera o wklejenie linku.</p>
          )}

          <ReportToolbar report={report} canManage={canManage} format={format} />
          <InsightList insights={report.insights} />
          <MapMatrix maps={report.maps} />
          <VetoPlanSection plans={report.vetoPlans} format={format} onFormatChange={setFormat} />
          <PlayersToWatch maps={report.playersToWatch} />
          <TeamFormSection form={report.form} />
          <MapTendenciesSection opponentName={report.opponentName} tendencies={report.tendencies ?? []} canManage={canManage} />
          <OpponentDemosSection opponentName={report.opponentName} canManage={canManage} />
        </>
      )}
    </div>
  )
}
