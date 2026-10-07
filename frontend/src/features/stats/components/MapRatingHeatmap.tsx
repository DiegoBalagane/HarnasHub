import { memo, useMemo } from 'react'
import type { AdvancedMapCell, AdvancedPlayer } from '../../../services/advancedStatsApi'
import { buildHeatmap, ratingColor } from '../advancedStats'

interface MapRatingHeatmapProps {
  players: AdvancedPlayer[]
  cells: AdvancedMapCell[]
}

/** Players x maps grid coloured by average rating; each cell shows the rating and the number of matches behind it. */
function MapRatingHeatmapInner({ players, cells }: MapRatingHeatmapProps) {
  const heatmap = useMemo(() => buildHeatmap(players, cells), [players, cells])

  if (heatmap.maps.length === 0) {
    return <p className="text-neutral-400">Brak statystyk z demek do zestawienia map.</p>
  }

  return (
    <div className="overflow-x-auto">
      <table className="text-sm">
        <thead className="text-neutral-500">
          <tr>
            <th className="pb-2 pr-3 text-left font-normal">Gracz</th>
            {heatmap.maps.map((map) => (
              <th key={map} className="px-1 pb-2 font-normal">
                {map}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {heatmap.rows.map(({ player, cells: row }) => (
            <tr key={player.userId}>
              <td className="py-0.5 pr-3 font-medium text-neutral-300">{player.name}</td>
              {heatmap.maps.map((map) => {
                const cell = row[map]
                return cell ? (
                  <td
                    key={map}
                    title={`${player.name} · ${map}: rating ${cell.avgRating.toFixed(2)}, ADR ${cell.avgAdr.toFixed(1)}, ${cell.matches} meczów`}
                    style={{ backgroundColor: ratingColor(cell.avgRating) }}
                    className="min-w-20 px-2 py-1 text-center text-neutral-50"
                  >
                    <span className="font-medium">{cell.avgRating.toFixed(2)}</span>
                    <span className="ml-1 text-xs text-neutral-300">({cell.matches})</span>
                  </td>
                ) : (
                  <td key={map} className="min-w-20 px-2 py-1 text-center text-neutral-600">
                    —
                  </td>
                )
              })}
            </tr>
          ))}
        </tbody>
      </table>
      <p className="mt-1 text-xs text-neutral-500">Rating (liczba meczów). Czerwony ≤ 0,70, zielony ≥ 1,30.</p>
    </div>
  )
}

export const MapRatingHeatmap = memo(MapRatingHeatmapInner)
