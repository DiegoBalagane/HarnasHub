import { useState } from 'react'
import { useCreateTournament, useDeleteTournament } from '../hooks/useTournaments'

interface TournamentPickerProps {
  tournaments: { id: string; name: string }[]
  value: string
  onChange: (tournamentId: string) => void
}

/** Selects an existing tournament to group a result under, creates a new one inline, or deletes the selected one. */
export function TournamentPicker({ tournaments, value, onChange }: TournamentPickerProps) {
  const [newName, setNewName] = useState('')
  const createTournament = useCreateTournament()
  const deleteTournament = useDeleteTournament()

  function handleCreate() {
    if (!newName.trim()) return
    createTournament.mutate(newName, {
      onSuccess: (tournament) => {
        onChange(tournament.id)
        setNewName('')
      },
    })
  }

  function handleDelete() {
    if (!value) return
    const tournament = tournaments.find((candidate) => candidate.id === value)
    if (!tournament) return
    if (!window.confirm(`Usunąć turniej „${tournament.name}"? Wyniki w nim zostaną, ale bez grupowania.`)) return
    deleteTournament.mutate(value, { onSuccess: () => onChange('') })
  }

  return (
    <div className="flex gap-2">
      <select
        required
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      >
        <option value="">Wybierz turniej…</option>
        {tournaments.map((tournament) => (
          <option key={tournament.id} value={tournament.id}>
            {tournament.name}
          </option>
        ))}
      </select>
      {value && (
        <button
          type="button"
          title="Usuń wybrany turniej"
          onClick={handleDelete}
          disabled={deleteTournament.isPending}
          className="shrink-0 rounded-md border border-neutral-700 px-2 py-2 text-sm text-neutral-400 transition hover:border-red-500 hover:text-red-400 disabled:opacity-50"
        >
          ✕
        </button>
      )}
      <input
        placeholder="Nowy turniej"
        value={newName}
        onChange={(event) => setNewName(event.target.value)}
        className="w-40 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-2 text-sm outline-none focus:border-neutral-500"
      />
      <button
        type="button"
        onClick={handleCreate}
        disabled={createTournament.isPending || !newName.trim()}
        className="shrink-0 rounded-md border border-neutral-700 px-3 py-2 text-sm text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
      >
        + Dodaj
      </button>
    </div>
  )
}
