import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { opponentNotesApi, type AddOpponentNotePayload } from '../../../services/opponentNotesApi'

/** Fetches scouting notes, optionally filtered by opponent name. */
export function useOpponentNotes(opponentName?: string) {
  return useQuery({
    queryKey: ['opponent-notes', opponentName],
    queryFn: () => opponentNotesApi.getNotes(opponentName),
  })
}

/** Adds a scouting note and refreshes the list. */
export function useAddOpponentNote() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: AddOpponentNotePayload) => opponentNotesApi.addNote(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['opponent-notes'] })
    },
  })
}
