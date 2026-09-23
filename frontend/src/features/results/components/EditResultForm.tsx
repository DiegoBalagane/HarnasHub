import { useState } from 'react'
import type { MapName } from '../../../services/nadesApi'
import { ApiError } from '../../../services/apiClient'
import type { MatchCategory, MatchResult } from '../../../services/resultsApi'
import { mapNames } from '../../nades/labels'
import { useLeagues } from '../hooks/useLeagues'
import { useTournaments } from '../hooks/useTournaments'
import { useUpdateResult } from '../hooks/useResults'
import { matchCategories, matchCategoryLabels } from '../labels'
import { LeaguePicker } from './LeaguePicker'
import { TournamentPicker } from './TournamentPicker'

const inputClass =
  'flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

/** Converts a UTC ISO timestamp to the local value a `datetime-local` input needs, since that input has no timezone
 * concept of its own — it always reads/writes in whatever the browser's local time is. */
function toLocalInputValue(isoUtc: string): string {
  const date = new Date(isoUtc)
  const offsetMs = date.getTimezoneOffset() * 60_000
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16)
}

interface EditResultFormProps {
  result: MatchResult
  onClose: () => void
}

/** Coach/Manager-only inline form for editing a logged result's metadata (opponent/score/map/date/category/etc.) —
 * never touches its already-imported player stat lines. */
export function EditResultForm({ result, onClose }: EditResultFormProps) {
  const [opponent, setOpponent] = useState(result.opponent)
  const [ourScore, setOurScore] = useState(String(result.ourScore))
  const [opponentScore, setOpponentScore] = useState(String(result.opponentScore))
  const [mapName, setMapName] = useState<MapName | ''>((result.mapName as MapName) ?? '')
  const [demoUrl, setDemoUrl] = useState(result.demoUrl ?? '')
  const [notes, setNotes] = useState(result.notes ?? '')
  const [playedAt, setPlayedAt] = useState(toLocalInputValue(result.playedAtUtc))
  const [category, setCategory] = useState<MatchCategory>(result.category)
  const [tournamentId, setTournamentId] = useState(result.tournamentId ?? '')
  const [leagueId, setLeagueId] = useState(result.leagueId ?? '')

  const updateResult = useUpdateResult()
  const { data: tournaments } = useTournaments()
  const { data: leagues } = useLeagues()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    updateResult.mutate(
      {
        matchResultId: result.id,
        payload: {
          opponent,
          ourScore: Number(ourScore),
          opponentScore: Number(opponentScore),
          mapName: mapName || null,
          demoUrl: demoUrl || null,
          notes: notes || null,
          playedAtUtc: new Date(playedAt).toISOString(),
          category,
          tournamentId: category === 'Tournament' ? tournamentId || null : null,
          leagueId: category === 'League' ? leagueId || null : null,
        },
      },
      { onSuccess: onClose },
    )
  }

  const canSubmit =
    opponent !== '' &&
    ourScore !== '' &&
    opponentScore !== '' &&
    (category !== 'Tournament' || tournamentId !== '') &&
    (category !== 'League' || leagueId !== '')

  return (
    <form
      onSubmit={handleSubmit}
      onClick={(event) => event.stopPropagation()}
      className="mt-3 flex flex-col gap-3 rounded-md border border-neutral-700 bg-neutral-950 p-3"
    >
      <div className="flex items-center justify-between">
        <h4 className="text-sm font-medium">Edytuj wynik</h4>
        <button type="button" onClick={onClose} className="text-xs text-neutral-500 hover:text-neutral-300">
          Zamknij
        </button>
      </div>

      <div className="flex flex-wrap gap-3">
        <input
          required
          placeholder="Przeciwnik"
          value={opponent}
          onChange={(event) => setOpponent(event.target.value)}
          className={inputClass}
        />
        <input
          required
          type="number"
          min={0}
          placeholder="Nasz wynik"
          value={ourScore}
          onChange={(event) => setOurScore(event.target.value)}
          className="w-24 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <input
          required
          type="number"
          min={0}
          placeholder="Wynik przeciwnika"
          value={opponentScore}
          onChange={(event) => setOpponentScore(event.target.value)}
          className="w-24 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
      </div>

      <div className="flex flex-wrap gap-3">
        <select
          value={category}
          onChange={(event) => {
            setCategory(event.target.value as MatchCategory)
            setTournamentId('')
            setLeagueId('')
          }}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          {matchCategories.map((option) => (
            <option key={option} value={option}>
              {matchCategoryLabels[option]}
            </option>
          ))}
        </select>

        <select
          value={mapName}
          onChange={(event) => setMapName(event.target.value as MapName | '')}
          className={inputClass}
        >
          <option value="">Mapa (opcjonalnie)</option>
          {mapNames.map((map) => (
            <option key={map} value={map}>
              {map}
            </option>
          ))}
        </select>

        <input
          type="datetime-local"
          value={playedAt}
          onChange={(event) => setPlayedAt(event.target.value)}
          className={inputClass}
        />
      </div>

      {category === 'Tournament' && (
        <TournamentPicker tournaments={tournaments ?? []} value={tournamentId} onChange={setTournamentId} />
      )}

      {category === 'League' && <LeaguePicker leagues={leagues ?? []} value={leagueId} onChange={setLeagueId} />}

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

      {updateResult.isError && (
        <p className="text-sm text-red-400">
          {updateResult.error instanceof ApiError ? updateResult.error.message : 'Nie udało się zapisać zmian.'}
        </p>
      )}

      <button
        type="submit"
        disabled={updateResult.isPending || !canSubmit}
        className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {updateResult.isPending ? 'Zapisywanie…' : 'Zapisz zmiany'}
      </button>
    </form>
  )
}
