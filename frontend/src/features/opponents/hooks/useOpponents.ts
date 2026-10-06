import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { opponentsApi, type OpponentNotePayload } from '../../../services/opponentsApi'

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
