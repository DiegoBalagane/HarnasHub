import { useMutation, useQueryClient } from '@tanstack/react-query'
import { analysisBoardsApi } from '../../../services/analysisBoardsApi'
import { nadesApi, type MapName } from '../../../services/nadesApi'
import type { GrenadeCluster } from '../../../services/opponentDemosApi'
import { clusterStrokes, grenadeLabels, libraryGrenadeType } from '../tendencyLabels'

/** Saves the opponent's grenade clusters on a map as a new analysis board (one coloured circle per cluster). */
export function useSaveClustersAsBoard() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      opponentName,
      mapName,
      clusters,
    }: {
      opponentName: string
      mapName: MapName
      clusters: GrenadeCluster[]
    }) =>
      analysisBoardsApi.createBoard({
        mapName,
        title: `Granaty rywala: ${opponentName}`.slice(0, 150),
        strokesJson: JSON.stringify(clusterStrokes(clusters)),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['analysis-boards'] })
    },
  })
}

/** Adds one opponent grenade cluster to the nade library (pinned at the cluster centre), marked as theirs in the description. */
export function useAddClusterToLibrary() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({
      opponentName,
      mapName,
      cluster,
    }: {
      opponentName: string
      mapName: MapName
      cluster: GrenadeCluster
    }) => {
      const type = libraryGrenadeType(cluster.type)
      if (!type) {
        throw new Error('Tego typu granatu nie da się dodać do biblioteki.')
      }

      const nade = await nadesApi.addNade({
        mapName,
        type,
        title:
          `Ich ${grenadeLabels[cluster.type]}${cluster.area ? ` ${cluster.area}` : ''} (${opponentName})`.slice(
            0,
            100,
          ),
        description: `Granat rywala ${opponentName} z analizy demek: ${cluster.rounds} rund T (${Math.round(cluster.perRoundPercent)}%), ${cluster.throws} rzutów.`,
      })
      return nadesApi.updateNadePosition({ nadeId: nade.id, x: cluster.x, y: cluster.y })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nades'] })
    },
  })
}
