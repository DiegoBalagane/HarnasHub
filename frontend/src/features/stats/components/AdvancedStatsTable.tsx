import { memo, useMemo, useState } from 'react'
import type { AdvancedPlayer } from '../../../services/advancedStatsApi'
import { advancedColumns, sortPlayers } from '../advancedStats'

interface AdvancedStatsTableProps {
  players: AdvancedPlayer[]
}

/** Sortable table of the per-player advanced metrics; every header carries a Polish tooltip explaining the metric. */
function AdvancedStatsTableInner({ players }: AdvancedStatsTableProps) {
  const [sortKey, setSortKey] = useState('rating')
  const [desc, setDesc] = useState(true)

  const sorted = useMemo(() => {
    const column = advancedColumns.find((c) => c.key === sortKey) ?? advancedColumns[0]
    return sortPlayers(players, column, desc)
  }, [players, sortKey, desc])

  function handleSort(key: string) {
    if (key === sortKey) {
      setDesc((current) => !current)
    } else {
      setSortKey(key)
      setDesc(true)
    }
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead className="text-neutral-500">
          <tr>
            <th className="pb-2 pr-3 font-normal">Gracz</th>
            {advancedColumns.map((column) => (
              <th
                key={column.key}
                title={column.title}
                aria-sort={sortKey === column.key ? (desc ? 'descending' : 'ascending') : 'none'}
                onClick={() => handleSort(column.key)}
                className={`cursor-pointer select-none whitespace-nowrap pb-2 pr-3 font-normal hover:text-neutral-300 ${
                  sortKey === column.key ? 'text-neutral-200' : ''
                }`}
              >
                {column.label}
                {sortKey === column.key ? (desc ? ' ↓' : ' ↑') : ''}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {sorted.map((player) => (
            <tr key={player.userId} className="border-t border-neutral-900 text-neutral-300">
              <td className="py-1.5 pr-3 font-medium">{player.name}</td>
              {advancedColumns.map((column) => (
                <td key={column.key} className="whitespace-nowrap pr-3">
                  {column.format(player)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export const AdvancedStatsTable = memo(AdvancedStatsTableInner)
