import { useState } from 'react'
import { useAddResult } from '../hooks/useResults'

/** Coach/Manager-only form for logging a scrim/match/tournament result. */
export function AddResultForm() {
  const [opponent, setOpponent] = useState('')
  const [ourScore, setOurScore] = useState('')
  const [opponentScore, setOpponentScore] = useState('')
  const [mapName, setMapName] = useState('')
  const [demoUrl, setDemoUrl] = useState('')
  const [notes, setNotes] = useState('')
  const [playedAt, setPlayedAt] = useState('')
  const addResult = useAddResult()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    addResult.mutate(
      {
        opponent,
        ourScore: Number(ourScore),
        opponentScore: Number(opponentScore),
        mapName: mapName || undefined,
        demoUrl: demoUrl || undefined,
        notes: notes || undefined,
        playedAtUtc: new Date(playedAt || Date.now()).toISOString(),
      },
      {
        onSuccess: () => {
          setOpponent('')
          setOurScore('')
          setOpponentScore('')
          setMapName('')
          setDemoUrl('')
          setNotes('')
          setPlayedAt('')
        },
      },
    )
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex w-full max-w-xl flex-col gap-3 rounded-md border border-neutral-800 p-4"
    >
      <h2 className="font-medium">Dodaj wynik</h2>

      <div className="flex gap-3">
        <input
          required
          placeholder="Przeciwnik"
          value={opponent}
          onChange={(event) => setOpponent(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <input
          required
          type="number"
          min={0}
          placeholder="Nasz wynik"
          value={ourScore}
          onChange={(event) => setOurScore(event.target.value)}
          className="w-28 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <input
          required
          type="number"
          min={0}
          placeholder="Wynik przeciwnika"
          value={opponentScore}
          onChange={(event) => setOpponentScore(event.target.value)}
          className="w-28 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
      </div>

      <div className="flex gap-3">
        <input
          placeholder="Mapa (opcjonalnie)"
          value={mapName}
          onChange={(event) => setMapName(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <input
          type="datetime-local"
          value={playedAt}
          onChange={(event) => setPlayedAt(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
      </div>

      <input
        placeholder="Link do demki (opcjonalnie)"
        value={demoUrl}
        onChange={(event) => setDemoUrl(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      <textarea
        placeholder="Notatki pomeczowe (opcjonalnie)"
        value={notes}
        onChange={(event) => setNotes(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {addResult.isError && <p className="text-sm text-red-400">Nie udało się dodać wyniku.</p>}

      <button
        type="submit"
        disabled={addResult.isPending}
        className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {addResult.isPending ? 'Dodawanie…' : 'Dodaj wynik'}
      </button>
    </form>
  )
}
