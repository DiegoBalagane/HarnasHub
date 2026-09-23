import { useRef, useState } from 'react'
import type { MapPosition } from '../../../services/mapStrategyApi'

interface DeletedNote {
  position: MapPosition
  previousNote: string
}

const undoWindowMs = 6000

/** Tracks the last deleted pin note for a few seconds so the coach can undo it — `saveNote` is the same
 * upsert used to write a note, called with `null` to delete and with the previous text to restore. */
export function useUndoableNoteDelete(saveNote: (position: MapPosition, note: string | null) => void) {
  const [deletedNote, setDeletedNote] = useState<DeletedNote | null>(null)
  const undoTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  function deleteNote(position: MapPosition) {
    if (!position.note) return

    if (undoTimeoutRef.current) {
      clearTimeout(undoTimeoutRef.current)
    }

    saveNote(position, null)
    setDeletedNote({ position, previousNote: position.note })
    undoTimeoutRef.current = setTimeout(() => setDeletedNote(null), undoWindowMs)
  }

  function undoDelete() {
    if (!deletedNote) return

    if (undoTimeoutRef.current) {
      clearTimeout(undoTimeoutRef.current)
    }

    saveNote(deletedNote.position, deletedNote.previousNote)
    setDeletedNote(null)
  }

  return { deletedNote, deleteNote, undoDelete }
}
