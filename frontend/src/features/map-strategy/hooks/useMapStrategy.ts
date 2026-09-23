import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  mapStrategyApi,
  type AddTextAnnotationPayload,
  type MapSide,
  type SetPlayerPositionPayload,
  type UpdateTextAnnotationPayload,
} from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'

/** Fetches every player's starting spot for one map and side. */
export function useMapPositions(mapName: MapName, side: MapSide) {
  return useQuery({
    queryKey: ['map-strategy', mapName, side],
    queryFn: () => mapStrategyApi.getPositions(mapName, side),
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

/** Fetches every free-floating text annotation for one map and side. */
export function useMapTextAnnotations(mapName: MapName, side: MapSide) {
  return useQuery({
    queryKey: ['map-strategy', 'text-annotations', mapName, side],
    queryFn: () => mapStrategyApi.getTextAnnotations(mapName, side),
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
