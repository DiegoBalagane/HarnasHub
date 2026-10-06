import { memo } from 'react'
import type { MapTendencies } from '../../../services/opponentDemosApi'
import { confidenceLabels } from '../labels'
import { roleLabels } from '../tendencyLabels'
import { GrenadeClusterList } from './GrenadeClusterList'
import { TendencyRadar } from './TendencyRadar'
import { TendencySideColumns } from './TendencySideColumns'

interface MapTendencyCardProps {
  opponentName: string
  tendencies: MapTendencies
  canManage: boolean
}

const sideLabels = { T: 'T', CT: 'CT', Players: 'Gracze' } as const

/** "Tendencje (z N demek, M rund)" card of one map: anti-strat suggestions, mini radar, side distributions, grenades, players. */
export const MapTendencyCard = memo(function MapTendencyCard({
  opponentName,
  tendencies,
  canManage,
}: MapTendencyCardProps) {
  return (
    <article className="flex flex-col gap-4 rounded-lg border border-neutral-800 p-4">
      <header className="flex flex-wrap items-baseline justify-between gap-2">
        <h3 className="text-base font-semibold">
          {tendencies.mapName} — Tendencje (z {tendencies.demos} demek, {tendencies.rounds} rund)
        </h3>
        {!tendencies.hasZones && (
          <span className="text-xs text-warning-300">Mapa bez stref — strefy i ustawienia niedostępne.</span>
        )}
      </header>

      {tendencies.suggestions.length > 0 && (
        <ul className="flex flex-col gap-1">
          {tendencies.suggestions.map((suggestion) => (
            <li
              key={suggestion.kind + suggestion.text}
              className="rounded-md border border-warning-700/60 bg-warning-950/20 px-3 py-1.5 text-sm"
            >
              <span className="mr-2 rounded bg-neutral-800 px-1.5 py-0.5 text-xs">
                {sideLabels[suggestion.side]}
              </span>
              {suggestion.text}
              <span className="ml-2 text-xs text-neutral-400">
                ({suggestion.evidence}; pewność {confidenceLabels[suggestion.confidence]})
              </span>
            </li>
          ))}
        </ul>
      )}

      <div className="grid gap-4 lg:grid-cols-[minmax(0,24rem)_1fr]">
        <TendencyRadar tendencies={tendencies} />
        <div className="flex flex-col gap-2">
          <span className="text-xs uppercase tracking-wide text-neutral-500">Standardowe granaty T</span>
          <GrenadeClusterList
            opponentName={opponentName}
            mapName={tendencies.mapName}
            clusters={tendencies.t.grenadeClusters}
            canManage={canManage}
          />
        </div>
      </div>

      <TendencySideColumns t={tendencies.t} ct={tendencies.ct} />

      {tendencies.players.length > 0 && (
        <table className="w-full text-left text-xs">
          <thead className="text-neutral-500">
            <tr>
              <th className="py-1">Gracz</th>
              <th>Rola</th>
              <th>Rundy</th>
              <th>Otwarcia</th>
              <th>Wygrane otwarcia</th>
              <th>AWP</th>
              <th>Clutche</th>
            </tr>
          </thead>
          <tbody>
            {tendencies.players.map((player) => (
              <tr key={player.steamId64} className="border-t border-neutral-800">
                <td className="py-1">{player.name}</td>
                <td className="text-warning-300">{player.role ? roleLabels[player.role] : ''}</td>
                <td>{player.rounds}</td>
                <td>{Math.round(player.entryRate)}%</td>
                <td>{player.openingWinRate === null ? '—' : `${Math.round(player.openingWinRate)}%`}</td>
                <td>{player.awpKills}</td>
                <td>
                  {player.clutchWins}/{player.clutchAttempts}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </article>
  )
})
