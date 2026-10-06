import { memo, useMemo } from 'react'
import type { DemoNade } from '../../../../services/tacticsApi'
import { grenadeTypeColors, grenadeTypeLabels, grenadeTypeMarks } from '../../../nades/labels'
import { formatRoundTime, groupByThrower } from './demoImport'

interface DemoNadeSelectionListProps {
  grenades: DemoNade[]
  selectedIds: ReadonlySet<number>
  /** Sets the selection state of the given grenades at once (one grenade, or a whole player's). */
  onToggle: (grenadeIds: number[], selected: boolean) => void
}

/** Grenades grouped by player, with a player-level checkbox (all of their grenades) and one per grenade. */
export const DemoNadeSelectionList = memo(function DemoNadeSelectionList({
  grenades,
  selectedIds,
  onToggle,
}: DemoNadeSelectionListProps) {
  const groups = useMemo(() => groupByThrower(grenades), [grenades])

  if (groups.length === 0) {
    return <p className="text-sm text-neutral-500">Ta strona nie rzuciła w tej rundzie żadnego granatu.</p>
  }

  return (
    <ul className="flex max-h-72 flex-col gap-2 overflow-y-auto pr-1">
      {groups.map((group) => {
        const ids = group.grenades.map((grenade) => grenade.id)
        const selectedCount = ids.filter((id) => selectedIds.has(id)).length
        return (
          <li key={group.key} className="rounded-md border border-neutral-800 p-2">
            <label className="flex items-center gap-2 text-sm font-medium text-neutral-100">
              <input
                type="checkbox"
                checked={selectedCount === ids.length}
                ref={(input) => {
                  if (input) input.indeterminate = selectedCount > 0 && selectedCount < ids.length
                }}
                onChange={(event) => onToggle(ids, event.target.checked)}
              />
              {group.throwerName}
              <span className="text-xs font-normal text-neutral-500">
                {selectedCount}/{ids.length}
              </span>
            </label>

            <ul className="mt-1 flex flex-col gap-0.5 pl-6">
              {group.grenades.map((grenade) => (
                <li key={grenade.id}>
                  <label className="flex items-center gap-2 text-xs text-neutral-300">
                    <input
                      type="checkbox"
                      checked={selectedIds.has(grenade.id)}
                      onChange={(event) => onToggle([grenade.id], event.target.checked)}
                    />
                    <span
                      className={`flex h-4 w-4 items-center justify-center rounded-full text-[9px] font-bold text-neutral-950 ${grenadeTypeColors[grenade.type]}`}
                    >
                      {grenadeTypeMarks[grenade.type]}
                    </span>
                    {grenadeTypeLabels[grenade.type]}
                    <span className="tabular-nums text-neutral-500">{formatRoundTime(grenade.secondsIntoRound)}</span>
                  </label>
                </li>
              ))}
            </ul>
          </li>
        )
      })}
    </ul>
  )
})
