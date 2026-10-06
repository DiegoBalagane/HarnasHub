import { memo, useState } from 'react'
import { ConfirmDialog } from '../../../components/ConfirmDialog'
import { Modal } from '../../../components/Modal'
import type { OpponentNote } from '../../../services/opponentsApi'
import { useDeleteOpponentNote } from '../hooks/useOpponents'
import { OpponentNoteForm } from './OpponentNoteForm'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

interface OpponentNotesSectionProps {
  opponentName: string
  notes: OpponentNote[]
  canManage: boolean
}

/** Scouting notes on an opponent's profile; Coach/Manager can add, edit and delete them in place. */
export function OpponentNotesSection({ opponentName, notes, canManage }: OpponentNotesSectionProps) {
  const [isAdding, setIsAdding] = useState(false)
  const [editing, setEditing] = useState<OpponentNote | null>(null)
  const [deleting, setDeleting] = useState<OpponentNote | null>(null)
  const deleteNote = useDeleteOpponentNote()

  return (
    <section className="flex flex-col gap-3">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-medium">Notatki</h2>
        {canManage && (
          <button
            type="button"
            onClick={() => setIsAdding(true)}
            className="rounded-md bg-primary-500 px-3 py-1.5 text-sm font-medium text-primary-950 transition hover:bg-primary-400"
          >
            + Dodaj notatkę
          </button>
        )}
      </div>

      {notes.length === 0 && <p className="text-sm text-neutral-500">Brak notatek o tym przeciwniku.</p>}

      <ul className="flex flex-col gap-3">
        {notes.map((note) => (
          <NoteCard
            key={note.id}
            note={note}
            canManage={canManage}
            onEdit={() => setEditing(note)}
            onDelete={() => setDeleting(note)}
          />
        ))}
      </ul>

      {isAdding && (
        <Modal title={`Notatka — ${opponentName}`} onClose={() => setIsAdding(false)}>
          <OpponentNoteForm defaultOpponentName={opponentName} onDone={() => setIsAdding(false)} />
        </Modal>
      )}

      {editing && (
        <Modal title="Edytuj notatkę" onClose={() => setEditing(null)}>
          <OpponentNoteForm note={editing} onDone={() => setEditing(null)} />
        </Modal>
      )}

      {deleting && (
        <ConfirmDialog
          title="Usunąć notatkę?"
          message="Tej operacji nie można cofnąć."
          confirmLabel="Usuń"
          isConfirming={deleteNote.isPending}
          onConfirm={() => deleteNote.mutate(deleting.id, { onSuccess: () => setDeleting(null) })}
          onCancel={() => setDeleting(null)}
        />
      )}
    </section>
  )
}

interface NoteCardProps {
  note: OpponentNote
  canManage: boolean
  onEdit: () => void
  onDelete: () => void
}

/** One scouting note with its date, optional material link and Coach/Manager actions. */
const NoteCard = memo(function NoteCard({ note, canManage, onEdit, onDelete }: NoteCardProps) {
  return (
    <li className="rounded-md border border-neutral-800 p-4">
      <div className="flex items-center justify-between gap-2">
        <span className="text-sm text-neutral-500">{dateFormatter.format(new Date(note.createdAtUtc))}</span>
        {canManage && (
          <div className="flex gap-1">
            <button
              type="button"
              title="Edytuj notatkę"
              onClick={onEdit}
              className="rounded-md px-2 py-1 text-sm text-neutral-500 transition hover:bg-neutral-800 hover:text-neutral-200"
            >
              ✎
            </button>
            <button
              type="button"
              title="Usuń notatkę"
              onClick={onDelete}
              className="rounded-md px-2 py-1 text-sm text-neutral-500 transition hover:bg-neutral-800 hover:text-danger-400"
            >
              ✕
            </button>
          </div>
        )}
      </div>
      <p className="mt-1 whitespace-pre-wrap text-sm text-neutral-300">{note.content}</p>
      {note.materialUrl && (
        <a
          href={note.materialUrl}
          target="_blank"
          rel="noreferrer"
          className="mt-1 inline-block text-sm text-primary-400 hover:underline"
        >
          Materiał
        </a>
      )}
    </li>
  )
})
