import { memo } from 'react'
import type { PlayerForm } from '../../../services/opponentReportApi'
import { formArrows, formatKd, formTooltip, splitTitle, topMaps } from '../individualForm'

/** One player's individual form: ELO/level, form arrow, top map chips and a per-map mini table. */
export const PlayerFormCard = memo(function PlayerFormCard({ player }: { player: PlayerForm }) {
  const arrow = player.recentForm ? formArrows[player.recentForm.direction] : null

  return (
    <article className="flex flex-col gap-2 rounded-md border border-neutral-800 p-3" aria-label={player.nickname}>
      <header className="flex items-baseline justify-between gap-2">
        <span className="font-medium">
          {player.nickname}
          {arrow && player.recentForm && (
            <span className={`ml-1.5 ${arrow.className}`} title={formTooltip(player.recentForm)} aria-label={arrow.label}>
              {arrow.symbol}
            </span>
          )}
        </span>
        <span className="text-xs text-neutral-400 tabular-nums">
          {player.elo !== null ? `${player.elo} ELO` : '— ELO'}
          {player.skillLevel !== null && ` · lvl ${player.skillLevel}`}
        </span>
      </header>

      <p className="text-xs text-neutral-400 tabular-nums">
        {player.games} m. ({player.teamGames} drużynowo / {player.soloGames} solo) · K/D {formatKd(player.kdRatio)}
        {player.winRate !== null && ` · ${Math.round(player.winRate)}% W`}
        {player.adr !== null && ` · ADR ${Math.round(player.adr)}`}
      </p>

      {player.maps.length > 0 && (
        <ul className="flex flex-wrap gap-1" aria-label="Najczęstsze mapy">
          {topMaps(player).map((map) => (
            <li key={map.mapName} className="rounded-full border border-neutral-700 px-2 py-0.5 text-xs">
              {map.mapName} {Math.round(map.share)}%
            </li>
          ))}
        </ul>
      )}

      {player.maps.length > 0 && (
        <table className="w-full text-xs">
          <thead className="text-left text-neutral-500">
            <tr>
              <th className="py-0.5 font-normal">Mapa</th>
              <th className="py-0.5 font-normal">M.</th>
              <th className="py-0.5 font-normal">WR</th>
              <th className="py-0.5 font-normal">K/D</th>
              <th className="py-0.5 font-normal">ADR</th>
              <th className="py-0.5 font-normal">Druż./solo</th>
            </tr>
          </thead>
          <tbody className="tabular-nums">
            {player.maps.map((map) => (
              <tr key={map.mapName} className="border-t border-neutral-800">
                <td className="py-0.5">{map.mapName}</td>
                <td className="py-0.5">{map.games}</td>
                <td className="py-0.5">{Math.round(map.winRate)}%</td>
                <td className="py-0.5">{formatKd(map.kdRatio)}</td>
                <td className="py-0.5">{map.adr === null ? '—' : Math.round(map.adr)}</td>
                <td className="py-0.5 text-neutral-400" title={splitTitle(map)}>
                  {map.teamGames}/{map.soloGames}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </article>
  )
})
