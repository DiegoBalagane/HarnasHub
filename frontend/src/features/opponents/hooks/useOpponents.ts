import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { opponentManagementApi, opponentsApi, type OpponentNotePayload } from '../../../services/opponentsApi'

/** Fetches every known opponent with the head-to-head record, upcoming games first. */
export function useOpponents() {
  return useQuery({
    queryKey: ['opponents', 'list'],
    queryFn: opponentsApi.getOpponents,
  })
}

/** Fetches one opponent's profile; the key is lower-cased so differently-typed spellings share one cache entry. */
export function useOpponentProfile(name: string) {
  return useQuery({
    queryKey: ['opponents', 'profile', name.trim().toLowerCase()],
    queryFn: () => opponentsApi.getProfile(name),
    enabled: name.trim() !== '',
  })
}

/** Refreshes both the opponent list and every cached profile after a note change. */
function useInvalidateOpponents() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: ['opponents'] })
}

/** Adds a scouting note. */
export function useAddOpponentNote() {
  const invalidate = useInvalidateOpponents()

  return useMutation({
    mutationFn: (payload: OpponentNotePayload) => opponentsApi.addNote(payload),
    onSuccess: invalidate,
  })
}

/** Edits a scouting note in place. */
export function useUpdateOpponentNote() {
  const invalidate = useInvalidateOpponents()

  return useMutation({
    mutationFn: ({ noteId, payload }: { noteId: string; payload: OpponentNotePayload }) =>
      opponentsApi.updateNote(noteId, payload),
    onSuccess: invalidate,
  })
}

/** Permanently deletes a scouting note. */
export function useDeleteOpponentNote() {
  const invalidate = useInvalidateOpponents()

  return useMutation({
    mutationFn: (noteId: string) => opponentsApi.deleteNote(noteId),
    onSuccess: invalidate,
  })
}

/** Fetches every opponent including hidden ones (flagged `isHidden`) — for the "Pokaż ukrytych" view. */
export function useOpponentsWithHidden(enabled: boolean) {
  return useQuery({
    queryKey: ['opponents', 'list', 'with-hidden'],
    queryFn: opponentManagementApi.getHiddenIncluded,
    enabled,
  })
}

/** Counts what deleting an opponent would remove; fetched only while the dialog is open. */
export function useOpponentDeletePreview(name: string, enabled: boolean) {
  return useQuery({
    queryKey: ['opponents', 'delete-preview', name.trim().toLowerCase()],
    queryFn: () => opponentManagementApi.getDeletePreview(name),
    enabled: enabled && name.trim() !== '',
    gcTime: 0,
  })
}

/** Refreshes everything an opponent change can touch: opponents, results, calendar, dashboard, stats and veto. */
function useInvalidateOpponentData() {
  const queryClient = useQueryClient()
  return () =>
    Promise.all(
      ['opponents', 'results', 'calendar', 'dashboard', 'stats', 'veto'].map((key) =>
        queryClient.invalidateQueries({ queryKey: [key] }),
      ),
    )
}

/** Deletes an opponent's scouting data, optionally with its results and events (otherwise it is hidden). */
export function useDeleteOpponent() {
  const invalidate = useInvalidateOpponentData()

  return useMutation({
    mutationFn: ({ name, includeHistory }: { name: string; includeHistory: boolean }) =>
      opponentManagementApi.remove(name, includeHistory),
    onSuccess: invalidate,
  })
}

/** Hides an opponent from the list and name suggestions. */
export function useHideOpponent() {
  const invalidate = useInvalidateOpponentData()

  return useMutation({
    mutationFn: (name: string) => opponentManagementApi.hide(name),
    onSuccess: invalidate,
  })
}

/** Restores a hidden opponent. */
export function useUnhideOpponent() {
  const invalidate = useInvalidateOpponentData()

  return useMutation({
    mutationFn: (name: string) => opponentManagementApi.unhide(name),
    onSuccess: invalidate,
  })
}

/** Renames an opponent, merging it into the target when that name already exists. */
export function useRenameOpponent() {
  const invalidate = useInvalidateOpponentData()

  return useMutation({
    mutationFn: ({ from, to }: { from: string; to: string }) => opponentManagementApi.rename(from, to),
    onSuccess: invalidate,
  })
}
