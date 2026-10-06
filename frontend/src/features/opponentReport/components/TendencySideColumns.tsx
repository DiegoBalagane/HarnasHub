import { memo } from 'react'
import type { CtSideTendencies, TSideTendencies } from '../../../services/opponentDemosApi'
import { confidenceLabels } from '../labels'
import { buyLabels, formatRoundTime, timingLabels } from '../tendencyLabels'
import { ShareBars } from './ShareBars'

interface TendencySideColumnsProps {
  t: TSideTendencies
  ct: CtSideTendencies
}

/** Confidence badge of one side's sample. */
function SideHeader({
  title,
  rounds,
  confidence,
}: {
  title: string
  rounds: number
  confidence: keyof typeof confidenceLabels
}) {
  return (
    <div className="flex items-center justify-between">
      <span className="font-medium">{title}</span>
      <span className="rounded bg-neutral-800 px-2 py-0.5 text-xs text-neutral-300">
        {rounds} rund · pewność {confidenceLabels[confidence]}
      </span>
    </div>
  )
}

/** T-side and CT-side distributions of one map's tendencies, side by side. */
export const TendencySideColumns = memo(function TendencySideColumns({ t, ct }: TendencySideColumnsProps) {
  const pistol = t.pistol

  return (
    <div className="grid gap-4 md:grid-cols-2">
      <div className="flex flex-col gap-3">
        <SideHeader title="Ich strona T" rounds={t.rounds} confidence={t.confidence} />
        <ShareBars title="Cel rundy (plant / pierwszy kontakt)" shares={t.targets} />
        <ShareBars title="Tempo rozegrania" shares={t.execTiming} labels={timingLabels} />
        {t.averageExecSecond !== null && (
          <span className="text-xs text-neutral-400">
            Średnio rozgrywają rundę w {formatRoundTime(t.averageExecSecond)} od startu.
          </span>
        )}
        <span className="text-xs text-neutral-400">
          Pistolówki: {pistol.pistolWins}/{pistol.pistolRounds} wygranych
          {pistol.tPistolTargets.length > 0 &&
            ` · na T idą: ${pistol.tPistolTargets.map((s) => `${s.label} ${Math.round(s.percent)}%`).join(', ')}`}
        </span>
        {pistol.lostPistols > 0 && (
          <ShareBars
            title={`Po przegranej pistolówce (${pistol.lostPistols})`}
            shares={pistol.afterLostPistolBuys}
            labels={buyLabels}
          />
        )}
      </div>

      <div className="flex flex-col gap-3">
        <SideHeader title="Ich strona CT" rounds={ct.rounds} confidence={ct.confidence} />
        <ShareBars title="Ustawienie w 0:20" shares={ct.setups} />
        <ShareBars title="Stacki" shares={ct.stacks} />
        <ShareBars title="Zabójstwa z AWP (strefa)" shares={ct.awpAreas} />
        <span className="text-xs text-neutral-400">
          Agresja: zabójstwo przed 0:25 w {Math.round(ct.earlyKillPercent)}% rund ({ct.earlyKillRounds}/
          {ct.rounds}).
        </span>
        {ct.postPlantRounds > 0 && (
          <span className="text-xs text-neutral-400">
            Po plancie: retake {ct.retakes} (wygrane {ct.retakesWon}), save {ct.saves} z {ct.postPlantRounds}{' '}
            rund.
          </span>
        )}
      </div>
    </div>
  )
})
