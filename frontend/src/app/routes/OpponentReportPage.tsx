import { useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'
import { ActiveLineupSection } from '../../features/opponentReport/components/ActiveLineupSection'
import { FaceitLinkForm } from '../../features/opponentReport/components/FaceitLinkForm'
import { IndividualFormSection } from '../../features/opponentReport/components/IndividualFormSection'
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

/** Opponent report: FACEIT "them vs us" (TL;DR, map matrix, veto, players, form, individual form) plus tendencies from their demos. */
export function OpponentReportPage() {
  const [searchParams] = useSearchParams()
  const name = searchParams.get('name') ?? ''
  const canManage = useIsCoachOrManager()
  const isManager = useAuthStore((state) => state.role === 'Manager')
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
            {report.activeLineup ? (
              <ActiveLineupSection lineup={report.activeLineup} />
            ) : (
              report.link && (
                <p className="text-sm text-neutral-400">
                  FACEIT: {report.link.players.map((player) => player.nickname).join(', ')}
                </p>
              )
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
          <MapMatrix maps={report.maps} comfort={report.individualForm?.theirs.mapComfort} lineup={report.activeLineup} />
          <VetoPlanSection plans={report.vetoPlans} format={format} onFormatChange={setFormat} />
          <PlayersToWatch maps={report.playersToWatch} />
          <TeamFormSection form={report.form} />
          <IndividualFormSection
            form={report.individualForm}
            unresolved={report.unresolvedOurPlayers}
            canSetNickname={isManager}
          />
          <MapTendenciesSection opponentName={report.opponentName} tendencies={report.tendencies ?? []} canManage={canManage} />
          <OpponentDemosSection
            opponentName={report.opponentName}
            canManage={canManage}
            recentGames={report.form?.lastGames}
          />
        </>
      )}
    </div>
  )
}
