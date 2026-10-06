import { useCallback, useState } from 'react'
import { Modal } from '../../../components/Modal'
import type { MapPoolMap } from '../../../services/mapPoolApi'
import type { MapName } from '../../../services/nadesApi'
import type { MatchCategory } from '../../../services/resultsApi'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { matchCategories, matchCategoryLabels } from '../../results/labels'
import { useMapPool } from '../hooks/useMapPool'
import { MapPoolCard } from './MapPoolCard'
import { MapPoolEntryForm } from './MapPoolEntryForm'

interface MapPoolOverviewProps {
  /** When set, only this map's card is shown; otherwise the whole pool. */
  mapName?: MapName
}

/** The team's map pool: each map's status (pick/playable/learning/ban) next to how the team actually does on it. */
export function MapPoolOverview({ mapName }: MapPoolOverviewProps) {
  const canManage = useIsCoachOrManager()
  const [category, setCategory] = useState<MatchCategory | undefined>(undefined)
  const [editing, setEditing] = useState<MapPoolMap | null>(null)
  const { data: maps, isLoading, isError } = useMapPool(category)
  const handleEdit = useCallback((map: MapPoolMap) => setEditing(map), [])
  const visibleMaps = mapName ? maps?.filter((map) => map.mapName === mapName) : maps

  const chipClass = (active: boolean) =>
    `rounded-full border px-3 py-1 text-sm transition ${
      active
        ? 'border-primary-500 bg-primary-950/40 text-white'
        : 'border-neutral-800 text-neutral-400 hover:border-neutral-600'
    }`

  return (
    <div className="flex w-full flex-col gap-4">
      <div className="flex flex-wrap gap-2" role="group" aria-label="Filtr typu meczu">
        <button type="button" onClick={() => setCategory(undefined)} className={chipClass(category === undefined)}>
          Wszystkie mecze
        </button>
        {matchCategories.map((option) => (
          <button key={option} type="button" onClick={() => setCategory(option)} className={chipClass(category === option)}>
            {matchCategoryLabels[option]}
          </button>
        ))}
      </div>

      {isLoading && <p className="text-neutral-400">Ładowanie puli map…</p>}
      {isError && <p className="text-danger-400">Nie udało się pobrać puli map.</p>}

      <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {visibleMaps?.map((map) => (
          <MapPoolCard key={map.mapName} map={map} canManage={canManage} onEdit={handleEdit} />
        ))}
      </ul>

      {editing && (
        <Modal title={`${editing.mapName} — status w puli`} onClose={() => setEditing(null)}>
          <MapPoolEntryForm map={editing} onDone={() => setEditing(null)} />
        </Modal>
      )}
    </div>
  )
}
