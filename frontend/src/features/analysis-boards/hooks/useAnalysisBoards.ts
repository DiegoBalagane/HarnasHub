import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { analysisBoardsApi, type SaveBoardPayload } from '../../../services/analysisBoardsApi'
import type { MapName } from '../../../services/nadesApi'

/** Fetches saved analysis boards, optionally narrowed to one map, most recently updated first. */
export function useAnalysisBoards(mapName?: MapName) {
  return useQuery({
    queryKey: ['analysis-boards', mapName ?? 'all'],
    queryFn: () => analysisBoardsApi.getBoards(mapName),
  })
}

/** Saves a new board and refreshes the gallery. */
export function useCreateBoard() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: SaveBoardPayload) => analysisBoardsApi.createBoard(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['analysis-boards'] })
    },
  })
}

/** Edits an existing board's title/strokes/background in place and refreshes the gallery. */
export function useUpdateBoard() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ boardId, payload }: { boardId: string; payload: Omit<SaveBoardPayload, 'mapName'> }) =>
      analysisBoardsApi.updateBoard(boardId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['analysis-boards'] })
    },
  })
}

/** Deletes a board and refreshes the gallery. */
export function useDeleteBoard() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (boardId: string) => analysisBoardsApi.deleteBoard(boardId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['analysis-boards'] })
    },
  })
}
