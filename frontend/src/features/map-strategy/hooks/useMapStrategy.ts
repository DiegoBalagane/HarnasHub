import { useMutation, useQueries, useQueryClient } from '@tanstack/react-query'
import {
  mapStrategyApi,
  type AddTextAnnotationPayload,
  type SetPlayerPositionPayload,
  type SidedMapPosition,
  type SidedMapTextAnnotation,
  type UpdateTextAnnotationPayload,
} from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import { mapSides } from '../labels'

/** Fetches every player's starting spot for one map, both sides merged, each tagged with its side. */
export function useMapPositions(mapName: MapName) {
  return useQueries({
    queries: mapSides.map((side) => ({
      queryKey: ['map-strategy', mapName, side],
      queryFn: () => mapStrategyApi.getPositions(mapName, side),
    })),
    combine: (results) => ({
      data: results.every((result) => result.data)
        ? results.flatMap((result, index): SidedMapPosition[] =>
            (result.data ?? []).map((position) => ({ ...position, side: mapSides[index] })),
          )
        : undefined,
      isLoading: results.some((result) => result.isLoading),
      isError: results.some((result) => result.isError),
    }),
  })
}

/** Upserts a player's starting spot (Coach/Manager only) and refreshes the radar. */
export function useSetPlayerPosition() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: SetPlayerPositionPayload) => mapStrategyApi.setPosition(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['map-strategy'] })
    },
  })
}

/** Removes a player's starting spot (Coach/Manager only) and refreshes the radar. */
export function useRemovePlayerPosition() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (positionId: string) => mapStrategyApi.removePosition(positionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['map-strategy'] })
    },
  })
}

/** Fetches every free-floating text annotation for one map, both sides merged, each tagged with its side. */
export function useMapTextAnnotations(mapName: MapName) {
  return useQueries({
    queries: mapSides.map((side) => ({
      queryKey: ['map-strategy', 'text-annotations', mapName, side],
      queryFn: () => mapStrategyApi.getTextAnnotations(mapName, side),
    })),
    combine: (results) =>
      results.flatMap((result, index): SidedMapTextAnnotation[] =>
        (result.data ?? []).map((annotation) => ({ ...annotation, side: mapSides[index] })),
      ),
  })
}

/** Adds a text annotation (Coach/Manager only) and refreshes the radar. */
export function useAddTextAnnotation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: AddTextAnnotationPayload) => mapStrategyApi.addTextAnnotation(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['map-strategy'] })
    },
  })
}

/** Updates a text annotation's content, style, or position (Coach/Manager only) and refreshes the radar. */
export function useUpdateTextAnnotation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: UpdateTextAnnotationPayload) => mapStrategyApi.updateTextAnnotation(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['map-strategy'] })
    },
  })
}

/** Removes a text annotation (Coach/Manager only) and refreshes the radar. */
export function useRemoveTextAnnotation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (annotationId: string) => mapStrategyApi.removeTextAnnotation(annotationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['map-strategy'] })
    },
  })
}
