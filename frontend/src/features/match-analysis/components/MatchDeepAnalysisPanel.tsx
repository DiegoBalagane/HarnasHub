import { memo } from 'react'
import type { MatchAnalysis } from '../../../services/matchAnalysisApi'
import type { MapName } from '../../../services/nadesApi'
import { isMissingTimeline, useMatchDeepAnalysis } from '../hooks/useMatchAnalysis'
import { GrenadeLibrarySection } from './GrenadeLibrarySection'
import { OpeningDuelsMap } from './OpeningDuelsMap'

interface MatchDeepAnalysisPanelProps {
  matchResultId: string
}

const percent = (part: number, total: number) => (total === 0 ? '—' : `${Math.round((100 * part) / total)}%`)

function Stat({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <div className="rounded-md border border-neutral-800 p-3">
      <p className="text-xs text-neutral-500">{label}</p>
      <p className="text-xl font-semibold">{value}</p>
      {hint && <p className="text-xs text-neutral-500">{hint}</p>}
    </div>
  )
}

function SummarySection({ analysis }: { analysis: MatchAnalysis }) {
  const { trades, clutches, flashes } = analysis
  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-semibold">Trade’y, clutche i flashe</h2>
      <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
        <Stat
          label="Zgony pomszczone przez kolegę"
          value={percent(trades.ourTradedDeaths, trades.ourDeaths)}
          hint={`${trades.ourTradedDeaths} z ${trades.ourDeaths} naszych zgonów kolega zabił zabójcę w ≤ 5 s (rywale: ${percent(trades.opponentTradedDeaths, trades.opponentDeaths)})`}
        />
        <Stat label="Pomszczenia (trade)" value={String(trades.ourTradeKills)} hint="zabity rywal, który chwilę wcześniej (≤ 5 s) zabił naszego" />
        <Stat
          label="Clutche"
          value={`${clutches.ourWon}/${clutches.ourAttempts}`}
          hint={`rywale: ${clutches.opponentWon}/${clutches.opponentAttempts}`}
        />
        <Stat
          label="Flashe"
          value={flashes.hasData ? String(flashes.enemiesFlashed) : '—'}
          hint={flashes.hasData ? `wrogów oślepionych, swoich: ${flashes.teamFlashes}` : 'brak danych — podmień demkę'}
        />
      </div>
      {clutches.clutches.some((clutch) => clutch.isOurs) && (
        <ul className="flex flex-wrap gap-2 text-xs">
          {clutches.clutches
            .filter((clutch) => clutch.isOurs)
            .map((clutch) => (
              <li
                key={`${clutch.roundNumber}-${clutch.steamId64}`}
                className={`rounded-md border px-2 py-1 ${
                  clutch.won ? 'border-success-900/60 text-success-300' : 'border-danger-900/60 text-danger-300'
                }`}
              >
                R{clutch.roundNumber}: {clutch.name} 1v{clutch.versus} — {clutch.won ? 'wygrany' : 'przegrany'}
              </li>
            ))}
        </ul>
      )}
    </section>
  )
}

function OpeningsSection({ analysis }: { analysis: MatchAnalysis }) {
  const { openings, mapName } = analysis
  if (openings.duels.length === 0) {
    return null
  }

  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-semibold">Otwierające pojedynki</h2>
      <div className="grid gap-4 md:grid-cols-2">
        {mapName && <OpeningDuelsMap mapName={mapName} duels={openings.duels} />}
        <ul className="flex flex-col gap-1 text-sm">
          {openings.buckets.map((bucket) => (
            <li key={`${bucket.side}-${bucket.zone}`} className="flex justify-between rounded-md border border-neutral-800 px-3 py-1.5">
              <span>
                {bucket.side} · {bucket.zone ?? 'poza strefami'}
              </span>
              <span className="text-neutral-300">
                {bucket.won}–{bucket.lost} ({percent(bucket.won, bucket.won + bucket.lost)})
              </span>
            </li>
          ))}
        </ul>
      </div>
      <p className="text-xs text-neutral-500">Zielone punkty — wygrane otwarcia, czerwone — przegrane (miejsce śmierci ofiary).</p>
    </section>
  )
}

/** Deep match analysis shown under the insights: trades, clutches, flashes, opening-duel mini-radar and grenades vs library. */
export const MatchDeepAnalysisPanel = memo(function MatchDeepAnalysisPanel({ matchResultId }: MatchDeepAnalysisPanelProps) {
  const { data: analysis, isLoading, error } = useMatchDeepAnalysis(matchResultId)

  if (isLoading) {
    return <p className="text-sm text-neutral-400">Ładowanie analizy…</p>
  }

  if (isMissingTimeline(error)) {
    return null
  }

  if (error) {
    return <p className="text-sm text-danger-400">{error.message}</p>
  }

  if (!analysis) {
    return null
  }

  return (
    <div className="flex flex-col gap-6">
      <SummarySection analysis={analysis} />
      <OpeningsSection analysis={analysis} />
      {analysis.grenades && analysis.mapName && (
        <GrenadeLibrarySection mapName={analysis.mapName as MapName} comparison={analysis.grenades} />
      )}
    </div>
  )
})
