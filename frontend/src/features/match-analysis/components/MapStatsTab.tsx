import { memo } from 'react'
import type { MapAnalytics, SiteStat, WinRate } from '../../../services/matchAnalysisApi'
import type { MapName } from '../../../services/nadesApi'
import { useMapAnalytics } from '../hooks/useMatchAnalysis'
import { buyTypeLabels, insightToneStyles } from '../labels'
import { TacticEffectivenessSection } from '../../tactics/components/TacticEffectivenessSection'

interface MapStatsTabProps {
  mapName: MapName | undefined
}

const percent = (part: number, total: number) => (total === 0 ? '—' : `${Math.round((100 * part) / total)}%`)

function RateCard({ label, rate }: { label: string; rate: WinRate }) {
  return (
    <div className="rounded-md border border-neutral-800 p-3">
      <p className="text-xs text-neutral-500">{label}</p>
      <p className="text-xl font-semibold">{percent(rate.won, rate.total)}</p>
      <p className="text-xs text-neutral-500">{rate.won}/{rate.total} rund</p>
    </div>
  )
}

function SiteList({ title, sites, unit }: { title: string; sites: SiteStat[]; unit: string }) {
  const total = sites.reduce((sum, site) => sum + site.total, 0)
  return (
    <div className="rounded-md border border-neutral-800 p-3">
      <h3 className="text-sm font-medium">{title}</h3>
      {sites.length === 0 ? (
        <p className="mt-1 text-sm text-neutral-500">Brak danych.</p>
      ) : (
        <ul className="mt-1 flex flex-col gap-1 text-sm text-neutral-300">
          {sites.map((site) => (
            <li key={site.site ?? 'unknown'} className="flex justify-between">
              <span>
                {site.site ? `Site ${site.site}` : 'Nieznany site'} · {site.total} {unit} ({percent(site.total, total)})
              </span>
              <span>wygrane {percent(site.won, site.total)}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

function Content({ data }: { data: MapAnalytics }) {
  return (
    <div className="flex flex-col gap-5">
      <p className="text-sm text-neutral-400">
        Na podstawie {data.matchesAnalyzed} meczów z demką ({data.roundsAnalyzed} rund)
        {data.matchesSkipped > 0 && `, pominięto ${data.matchesSkipped} (brak rozpoznanej drużyny lub pliku)`}.
      </p>

      {data.insights.length > 0 && (
        <ul className="grid gap-2 md:grid-cols-2">
          {data.insights.map((insight) => (
            <li key={insight.code} className={`rounded-md border p-3 ${insightToneStyles[insight.tone]}`}>
              <p className="font-medium">{insight.title}</p>
              <p className="mt-1 text-sm text-neutral-400">{insight.detail}</p>
            </li>
          ))}
        </ul>
      )}

      <div className="grid gap-2 sm:grid-cols-3">
        <RateCard label="Win rate T" rate={data.tSide} />
        <RateCard label="Win rate CT" rate={data.ctSide} />
        <RateCard label="Pistolówki" rate={data.pistol} />
      </div>

      <div className="grid gap-2 md:grid-cols-2">
        <SiteList title="Wejścia na bombsite (T)" sites={data.tSites} unit="plantów" />
        <SiteList title="Retake na CT" sites={data.ctRetakes} unit="plantów rywala" />
      </div>

      <div className="grid gap-2 md:grid-cols-2">
        <div className="rounded-md border border-neutral-800 p-3">
          <h3 className="text-sm font-medium">Skuteczność wg zakupu</h3>
          <ul className="mt-1 flex flex-col gap-1 text-sm text-neutral-300">
            {data.buyTypes.map((buy) => (
              <li key={buy.buyType} className="flex justify-between">
                <span>{buyTypeLabels[buy.buyType]}</span>
                <span>
                  {buy.won}/{buy.total} ({percent(buy.won, buy.total)})
                </span>
              </li>
            ))}
          </ul>
        </div>
        <div className="rounded-md border border-neutral-800 p-3">
          <h3 className="text-sm font-medium">Trade’y i clutche</h3>
          <p className="mt-1 text-sm text-neutral-300">
            Odpłacone zgony: {data.trades.ourTradedDeaths}/{data.trades.ourDeaths} (
            {percent(data.trades.ourTradedDeaths, data.trades.ourDeaths)})
          </p>
          <ul className="mt-1 text-sm text-neutral-400">
            {data.topClutchers.map((player) => (
              <li key={player.steamId64}>
                {player.name}: {player.won}/{player.attempts} clutchy
              </li>
            ))}
          </ul>
        </div>
      </div>

      {data.openings.length > 0 && (
        <div className="rounded-md border border-neutral-800 p-3">
          <h3 className="text-sm font-medium">Otwierające pojedynki wg strefy</h3>
          <ul className="mt-1 grid gap-1 text-sm text-neutral-300 md:grid-cols-2">
            {data.openings.map((zone) => (
              <li key={`${zone.side}-${zone.zone}`} className="flex justify-between">
                <span>
                  {zone.side} · {zone.zone ?? 'poza strefami'}
                </span>
                <span>
                  {zone.won}–{zone.lost} ({percent(zone.won, zone.won + zone.lost)})
                </span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}

/** Playbook "Statystyki" tab: multi-match aggregates and insights for the selected map; asks for a map when none is selected. */
export const MapStatsTab = memo(function MapStatsTab({ mapName }: MapStatsTabProps) {
  if (!mapName) {
    return <p className="text-sm text-neutral-400">Wybierz mapę powyżej, aby zobaczyć statystyki z analizowanych demek.</p>
  }

  return (
    <div className="flex flex-col gap-4">
      <MapAnalyticsBody mapName={mapName} />
      <TacticEffectivenessSection mapName={mapName} />
    </div>
  )
})

/** Multi-match aggregates of the selected map, or why there are none. */
function MapAnalyticsBody({ mapName }: { mapName: MapName }) {
  const { data, isLoading, error } = useMapAnalytics(mapName)

  if (isLoading) {
    return <p className="text-sm text-neutral-400">Ładowanie statystyk…</p>
  }

  if (error) {
    return <p className="text-sm text-danger-400">{error.message}</p>
  }

  if (!data || data.matchesAnalyzed === 0) {
    return <p className="text-sm text-neutral-400">Brak przeanalizowanych demek na tej mapie — dołącz demkę do wyniku meczu.</p>
  }

  return <Content data={data} />
}
