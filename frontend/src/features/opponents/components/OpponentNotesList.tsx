import { useState } from 'react'
import { useOpponentNotes } from '../hooks/useOpponentNotes'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

/** Lists scouting notes with a filter by opponent name. */
export function OpponentNotesList() {
  const [filter, setFilter] = useState('')
  const { data: notes, isLoading, isError } = useOpponentNotes(filter || undefined)

  return (
    <div className="flex w-full max-w-xl flex-col gap-4">
      <input
        placeholder="Filtruj po nazwie przeciwnika…"
        value={filter}
        onChange={(event) => setFilter(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {isLoading && <p className="text-neutral-400">Ładowanie notatek…</p>}
      {isError && <p className="text-red-400">Nie udało się pobrać notatek.</p>}
      {notes?.length === 0 && <p className="text-neutral-400">Brak notatek.</p>}

      <ul className="flex flex-col gap-3">
        {notes?.map((note) => (
          <li key={note.id} className="rounded-md border border-neutral-800 p-4">
            <div className="flex items-center justify-between">
              <p className="font-medium">{note.opponentName}</p>
              <span className="text-sm text-neutral-500">
                {dateFormatter.format(new Date(note.createdAtUtc))}
              </span>
            </div>
            <p className="mt-1 whitespace-pre-wrap text-sm text-neutral-400">{note.content}</p>
            {note.materialUrl && (
              <a
                href={note.materialUrl}
                target="_blank"
                rel="noreferrer"
                className="mt-1 inline-block text-sm text-red-400 hover:underline"
              >
                Materiał
              </a>
            )}
          </li>
        ))}
      </ul>
    </div>
  )
}
