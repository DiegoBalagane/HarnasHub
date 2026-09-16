import { useState } from 'react'
import type { MapPosition } from '../../../services/mapStrategyApi'

interface PositionNoteEditorProps {
  position: MapPosition
  onSave: (note: string | null) => void
  onClose: () => void
  isSaving: boolean
}

/** Small inline form for the instruction note on one pin — what to do/play at that spot. */
export function PositionNoteEditor({ position, onSave, onClose, isSaving }: PositionNoteEditorProps) {
  const [note, setNote] = useState(position.note ?? '')
  const name = position.inGameNickname ?? position.displayName

  return (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-700 bg-neutral-950 p-3">
      <div className="flex items-center justify-between">
        <p className="text-xs text-neutral-400">Notatka dla: {name}</p>
        <button type="button" onClick={onClose} className="text-xs text-neutral-500 hover:text-neutral-300">
          Zamknij
        </button>
      </div>

      <textarea
        maxLength={300}
        rows={2}
        placeholder="Co robić na tej pozycji…"
        value={note}
        onChange={(event) => setNote(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      <div className="flex gap-2">
        <button
          type="button"
          disabled={isSaving}
          onClick={() => onSave(note.trim() === '' ? null : note.trim())}
          className="self-start rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
        >
          Zapisz notatkę
        </button>
        {position.note && (
          <button
            type="button"
            disabled={isSaving}
            onClick={() => {
              setNote('')
              onSave(null)
            }}
            className="rounded-md border border-neutral-700 px-3 py-1.5 text-xs text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
          >
            Usuń notatkę
          </button>
        )}
      </div>
    </div>
  )
}
