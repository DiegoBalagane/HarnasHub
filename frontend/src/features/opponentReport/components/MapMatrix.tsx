import { memo } from 'react'
import type { ActiveLineup, MapComfort, MapComparison } from '../../../services/opponentReportApi'
import { vetoActionClasses, vetoRecommendationLabels } from '../../veto/labels'
import { comfortLabel, comfortTitle } from '../individualForm'
import { advantageClass, confidenceLabels, formatPercent, formatSigned, predictionLabels } from '../labels'
import { lifetimeLabel, lifetimeTitle, ourSampleNote, ourTitle, teamGamesSplitLabel } from '../lineup'

/** Map matrix: their games and win rate vs ours, their solo comfort, the smoothed advantage, its confidence, their expected move and our recommendation. */
export const MapMatrix = memo(function MapMatrix({
  maps,
  comfort = [],
  lineup,
}: {
  maps: MapComparison[]
  comfort?: MapComfort[]
  /** Their lineup — labels the official vs "together" split of their games. */
  lineup?: ActiveLineup | null
}) {
  const comfortByMap = new Map(comfort.map((c) => [c.mapName, c]))
  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-medium">Mapy: oni vs my</h2>
      <p className="text-xs text-neutral-500">
        Oni — mecze oficjalne ich drużyny FACEIT (ESEA, puchary; wszystkie sezony, nowsze ważą więcej) plus
        mecze, w których ≥ 3 graczy z aktualnego składu grało razem; członkowie drużyny spoza składu nie
        tworzą meczu drużynowego. My — FACEIT plus wyniki zapisane w HarnasHub. Komfort solo — ilu ocenionych
        graczy rywala gra mapę regularnie w meczach solo (≥ 3) i ilu jej unika (0–1). Przewaga liczona z
        wygładzonego WR (małe próbki ciągnięte do 50%, lekko przesunięte formą solo; nowsze mecze ważą
        więcej). Poniżej 5 meczów na mapie bilans jest tylko pokazywany („za mało danych”), a o naszych
        pickach/banach decyduje pula map. Doświadczenie (lifetime) — suma meczów i śr. K/D składu na FACEIT,
        jak w pokoju meczu; najniższa waga.
      </p>
      <div className="overflow-x-auto rounded-md border border-neutral-800">
        <table className="w-full text-sm">
          <thead className="bg-neutral-900 text-left text-xs text-neutral-400">
            <tr>
              <th className="px-3 py-2">Mapa</th>
              <th className="px-3 py-2">Oni</th>
              <th className="px-3 py-2">Komfort solo</th>
              <th className="px-3 py-2">My</th>
              <th className="px-3 py-2">Doświadczenie (lifetime) oni / my</th>
              <th className="px-3 py-2">Przewaga</th>
              <th className="px-3 py-2">Pewność</th>
              <th className="px-3 py-2">Oni prawdopodobnie</th>
              <th className="px-3 py-2">My</th>
            </tr>
          </thead>
          <tbody>
            {maps.map((map) => (
              <MapRow
                key={map.mapName}
                map={map}
                comfort={comfortByMap.get(map.mapName)}
                split={teamGamesSplitLabel(map, lineup)}
              />
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
})

/** One map of the matrix; reasons are available on hover. */
const MapRow = memo(function MapRow({
  map,
  comfort,
  split,
}: {
  map: MapComparison
  comfort: MapComfort | undefined
  split: string | null
}) {
  const comfortText = comfortLabel(comfort)
  const sampleNote = ourSampleNote(map)
  return (
    <tr className="border-t border-neutral-800">
      <td className="px-3 py-2 font-medium">{map.mapName}</td>
      <td className="px-3 py-2 tabular-nums" title={trendTitle(map)}>
        {map.theirGames} · {formatPercent(map.theirWinRate)}
        {map.theirGames > 0 && (
          <span className="ml-1 text-xs text-neutral-500">({Math.round(map.theirShare)}% meczów)</span>
        )}
        {map.theirGames > 0 && map.theirLowSample && (
          <span className="ml-1 text-xs text-warning-300/80">(mała próba)</span>
        )}
        {split && <span className="block text-xs text-neutral-500">{split}</span>}
      </td>
      <td
        className="px-3 py-2 text-xs text-neutral-400"
        title={comfort && comfortText ? comfortTitle(comfort) : undefined}
      >
        {comfortText ?? '—'}
      </td>
      <td className="px-3 py-2 tabular-nums" title={ourTitle(map)}>
        {map.ourGames} · {formatPercent(map.ourWinRate)}
        {sampleNote && <span className="ml-1 text-xs text-warning-300/80">({sampleNote})</span>}
      </td>
      <td className="px-3 py-2 text-xs tabular-nums text-neutral-400">
        <span
          title={lifetimeTitle(map.theirLifetime, 'oni')}
          className={map.theirLifetime?.experienced ? 'text-neutral-200' : undefined}
        >
          {lifetimeLabel(map.theirLifetime)}
        </span>
        <span className="mx-1 text-neutral-600">/</span>
        <span title={lifetimeTitle(map.ourLifetime, 'my')}>{lifetimeLabel(map.ourLifetime)}</span>
      </td>
      <td className={`px-3 py-2 font-medium tabular-nums ${advantageClass(map.advantage)}`}>
        {formatSigned(map.advantage)} pp
      </td>
      <td className="px-3 py-2 text-neutral-400">{confidenceLabels[map.confidence]}</td>
      <td className="px-3 py-2 text-neutral-300" title={map.predictionReason}>
        {predictionLabels[map.prediction]}
      </td>
      <td className="px-3 py-2" title={map.vetoReasons.join('\n')}>
        <span className={`rounded-full border px-2 py-0.5 text-xs ${vetoActionClasses[map.recommendation]}`}>
          {vetoRecommendationLabels[map.recommendation]}
        </span>
      </td>
    </tr>
  )
})

/** Tooltip with the round difference, trend and last game of their side. */
function trendTitle(map: MapComparison): string {
  if (map.theirGames === 0) return 'Nie grali tej mapy w ostatnich 120 dniach'
  const parts = [`Średnia różnica rund: ${map.theirAvgRoundDiff ?? 0}`]
  if (map.theirTrend !== null)
    parts.push(`Trend (ostatnie 5 vs wcześniej): ${formatSigned(map.theirTrend)} pp`)
  if (map.theirLastPlayedAtUtc)
    parts.push(`Ostatnio: ${new Date(map.theirLastPlayedAtUtc).toLocaleDateString('pl-PL')}`)
  return parts.join('\n')
}
