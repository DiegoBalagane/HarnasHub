import { memo } from 'react'
import type { MapName } from '../../../services/nadesApi'
import type { GrenadeCluster } from '../../../services/opponentDemosApi'
import { useAddClusterToLibrary, useSaveClustersAsBoard } from '../hooks/useTendencyActions'
import { grenadeColors, grenadeLabels, libraryGrenadeType } from '../tendencyLabels'

interface GrenadeClusterListProps {
  opponentName: string
  mapName: MapName
  clusters: GrenadeCluster[]
  canManage: boolean
}

/** The opponent's standard T grenades (clusters) with "save as board" and "add to library as theirs" actions. */
export const GrenadeClusterList = memo(function GrenadeClusterList({
  opponentName,
  mapName,
  clusters,
  canManage,
}: GrenadeClusterListProps) {
  const saveBoard = useSaveClustersAsBoard()
  const addNade = useAddClusterToLibrary()

  if (clusters.length === 0) {
    return <p className="text-xs text-neutral-500">Brak powtarzających się granatów.</p>
  }

  return (
    <div className="flex flex-col gap-2">
      <ul className="flex flex-col gap-1 text-xs">
        {clusters.map((cluster, i) => (
          <li key={i} className="flex items-center justify-between gap-2">
            <span>
              <span
                className="mr-1 inline-block h-2 w-2 rounded-full"
                style={{ backgroundColor: grenadeColors[cluster.type] }}
              />
              {grenadeLabels[cluster.type]}
              {cluster.area && ` · ${cluster.area}`} · {Math.round(cluster.perRoundPercent)}% rund T (
              {cluster.rounds})
            </span>
            {canManage && libraryGrenadeType(cluster.type) && (
              <button
                type="button"
                disabled={addNade.isPending}
                onClick={() => addNade.mutate({ opponentName, mapName, cluster })}
                className="text-primary-400 hover:underline disabled:opacity-50"
              >
                Dodaj do biblioteki jako ich granat
              </button>
            )}
          </li>
        ))}
      </ul>
      {canManage && (
        <button
          type="button"
          disabled={saveBoard.isPending}
          onClick={() => saveBoard.mutate({ opponentName, mapName, clusters })}
          className="self-start rounded-md border border-neutral-800 px-3 py-1 text-xs text-primary-400 hover:border-neutral-600 disabled:opacity-50"
        >
          Zapisz klastry granatów jako tablicę
        </button>
      )}
      {saveBoard.isSuccess && (
        <span className="text-xs text-success-400">Zapisano tablicę „{saveBoard.data.title}”.</span>
      )}
      {addNade.isSuccess && <span className="text-xs text-success-400">Dodano granat do biblioteki.</span>}
      {(saveBoard.isError || addNade.isError) && (
        <span className="text-xs text-danger-400">{(saveBoard.error ?? addNade.error)?.message}</span>
      )}
    </div>
  )
})
