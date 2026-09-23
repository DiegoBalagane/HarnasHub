import { useState } from 'react'
import type { LeagueType } from '../../../services/leaguesApi'
import { useCreateLeague } from '../hooks/useLeagues'
import { leagueTypeLabels, leagueTypes } from '../labels'

interface LeaguePickerProps {
  leagues: { id: string; name: string; season: string; type: LeagueType }[]
  value: string
  onChange: (leagueId: string) => void
}

/** Selects an existing league season to group a result under, or creates a new one inline. */
export function LeaguePicker({ leagues, value, onChange }: LeaguePickerProps) {
  const [newName, setNewName] = useState('')
  const [newSeason, setNewSeason] = useState('')
  const [newType, setNewType] = useState<LeagueType>('Online')
  const createLeague = useCreateLeague()

  function handleCreate() {
    if (!newName.trim() || !newSeason.trim()) return
    createLeague.mutate(
      { name: newName, season: newSeason, type: newType },
      {
        onSuccess: (league) => {
          onChange(league.id)
          setNewName('')
          setNewSeason('')
        },
      },
    )
  }

  return (
    <div className="flex flex-col gap-2">
      <select
        required
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      >
        <option value="">Wybierz ligę…</option>
        {leagues.map((league) => (
          <option key={league.id} value={league.id}>
            {league.name} — {league.season} ({leagueTypeLabels[league.type]})
          </option>
        ))}
      </select>
      <div className="flex gap-2">
        <input
          placeholder="Nowa liga: nazwa"
          value={newName}
          onChange={(event) => setNewName(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <input
          placeholder="Sezon (np. 2026 Wiosna)"
          value={newSeason}
          onChange={(event) => setNewSeason(event.target.value)}
          className="w-40 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <select
          value={newType}
          onChange={(event) => setNewType(event.target.value as LeagueType)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-2 text-sm outline-none focus:border-neutral-500"
        >
          {leagueTypes.map((type) => (
            <option key={type} value={type}>
              {leagueTypeLabels[type]}
            </option>
          ))}
        </select>
        <button
          type="button"
          onClick={handleCreate}
          disabled={createLeague.isPending || !newName.trim() || !newSeason.trim()}
          className="shrink-0 rounded-md border border-neutral-700 px-3 py-2 text-sm text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
        >
          + Dodaj
        </button>
      </div>
    </div>
  )
}
