import { useState } from 'react'
import { useAddOpponentNote } from '../hooks/useOpponentNotes'

/** Coach/Manager-only form for adding a scouting note about an opponent. */
export function AddOpponentNoteForm() {
  const [opponentName, setOpponentName] = useState('')
  const [content, setContent] = useState('')
  const [materialUrl, setMaterialUrl] = useState('')
  const addNote = useAddOpponentNote()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    addNote.mutate(
      { opponentName, content, materialUrl: materialUrl || undefined },
      {
        onSuccess: () => {
          setOpponentName('')
          setContent('')
          setMaterialUrl('')
        },
      },
    )
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex w-full max-w-xl flex-col gap-3 rounded-md border border-neutral-800 p-4"
    >
      <h2 className="font-medium">Dodaj notatkę o przeciwniku</h2>

      <input
        required
        placeholder="Nazwa przeciwnika"
        value={opponentName}
        onChange={(event) => setOpponentName(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      <textarea
        required
        placeholder="Notatka (styl gry, tendencje, słabe strony...)"
        value={content}
        onChange={(event) => setContent(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      <input
        placeholder="Link do materiału/demki (opcjonalnie)"
        value={materialUrl}
        onChange={(event) => setMaterialUrl(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {addNote.isError && <p className="text-sm text-red-400">Nie udało się dodać notatki.</p>}

      <button
        type="submit"
        disabled={addNote.isPending}
        className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {addNote.isPending ? 'Dodawanie…' : 'Dodaj notatkę'}
      </button>
    </form>
  )
}
