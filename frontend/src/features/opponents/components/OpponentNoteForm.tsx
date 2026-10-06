import { useState } from 'react'
import type { OpponentNote } from '../../../services/opponentsApi'
import { useAddOpponentNote, useUpdateOpponentNote } from '../hooks/useOpponents'
import { OpponentNameInput } from './OpponentNameInput'

const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

interface OpponentNoteFormProps {
  /** Note being edited; omitted when adding a new one. */
  note?: OpponentNote
  /** Pre-filled opponent for a new note (e.g. when adding from that opponent's profile). */
  defaultOpponentName?: string
  /** Called after a successful save — e.g. to close the hosting modal. */
  onDone?: () => void
}

/** Coach/Manager-only form for adding or editing a scouting note about an opponent. */
export function OpponentNoteForm({ note, defaultOpponentName, onDone }: OpponentNoteFormProps) {
  const [opponentName, setOpponentName] = useState(note?.opponentName ?? defaultOpponentName ?? '')
  const [content, setContent] = useState(note?.content ?? '')
  const [materialUrl, setMaterialUrl] = useState(note?.materialUrl ?? '')
  const addNote = useAddOpponentNote()
  const updateNote = useUpdateOpponentNote()
  const mutation = note ? updateNote : addNote

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    const payload = { opponentName, content, materialUrl: materialUrl || undefined }
    const options = { onSuccess: () => onDone?.() }

    if (note) {
      updateNote.mutate({ noteId: note.id, payload }, options)
    } else {
      addNote.mutate(payload, options)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex w-full flex-col gap-3">
      <OpponentNameInput required value={opponentName} onChange={setOpponentName} className={inputClass} />

      <textarea
        required
        rows={5}
        placeholder="Notatka (styl gry, tendencje, słabe strony...)"
        value={content}
        onChange={(event) => setContent(event.target.value)}
        className={inputClass}
      />

      <input
        type="url"
        placeholder="Link do materiału/demki (opcjonalnie)"
        value={materialUrl}
        onChange={(event) => setMaterialUrl(event.target.value)}
        className={inputClass}
      />

      {mutation.isError && <p className="text-sm text-danger-400">Nie udało się zapisać notatki.</p>}

      <button
        type="submit"
        disabled={mutation.isPending}
        className="self-start rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
      >
        {mutation.isPending ? 'Zapisywanie…' : note ? 'Zapisz zmiany' : 'Dodaj notatkę'}
      </button>
    </form>
  )
}
