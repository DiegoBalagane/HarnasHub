import { useEffect, useState } from 'react'
import type { MapName } from '../../../services/nadesApi'
import type { AnalyzeDemoResult, MatchCategory, MatchResult } from '../../../services/resultsApi'
import type { LeagueType } from '../../../services/leaguesApi'
import { ApiError } from '../../../services/apiClient'
import { mapNames } from '../../nades/labels'
import { useAddResult, useAnalyzeDemo } from '../hooks/useResults'
import { useCreateLeague, useLeagues } from '../hooks/useLeagues'
import { useCreateTournament, useTournaments } from '../hooks/useTournaments'
import { leagueTypeLabels, leagueTypes, matchCategories, matchCategoryLabels } from '../labels'

const inputClass =
  'flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

/** Coach/Manager-only form for logging a scrim/match/tournament result, grouped under a tournament or league when relevant.
 * A demo upload is a separate "analyse" step, done before the result is submitted — the map, score, and a stat line per
 * matched roster member are all prefilled from that preview, but every prefilled field stays freely editable, so whatever
 * ends up in the form at submit time (typed by hand or left as prefilled) is exactly what gets saved. */
export function AddResultForm() {
  const [opponent, setOpponent] = useState('')
  const [ourScore, setOurScore] = useState('')
  const [opponentScore, setOpponentScore] = useState('')
  const [mapName, setMapName] = useState<MapName | ''>('')
  const [demoUrl, setDemoUrl] = useState('')
  const [notes, setNotes] = useState('')
  const [playedAt, setPlayedAt] = useState('')
  const [category, setCategory] = useState<MatchCategory>('Scrimmage')
  const [tournamentId, setTournamentId] = useState('')
  const [leagueId, setLeagueId] = useState('')
  const [saved, setSaved] = useState<MatchResult | null>(null)
  // A file input keeps showing the chosen file name after its state is cleared — remounting it is the only way to reset it.
  const [demoInputKey, setDemoInputKey] = useState(0)
  const [elapsedSeconds, setElapsedSeconds] = useState(0)
  const [analysis, setAnalysis] = useState<AnalyzeDemoResult | null>(null)
  const [selectedTeam, setSelectedTeam] = useState<'A' | 'B' | ''>('')

  const addResult = useAddResult()
  const analyzeDemo = useAnalyzeDemo()
  const { data: tournaments } = useTournaments()
  const { data: leagues } = useLeagues()

  // A 245MB demo takes ~10-15s to upload and parse — without this, the form just looks frozen for that long.
  useEffect(() => {
    if (!analyzeDemo.isPending) {
      setElapsedSeconds(0)
      return
    }

    const startedAt = Date.now()
    const interval = setInterval(() => setElapsedSeconds(Math.floor((Date.now() - startedAt) / 1000)), 1000)
    return () => clearInterval(interval)
  }, [analyzeDemo.isPending])

  function handleDemoSelected(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    if (!file) return

    setAnalysis(null)
    setSelectedTeam('')
    analyzeDemo.mutate(file, {
      onSuccess: (result) => {
        setAnalysis(result)
        if (result.mapName) setMapName(result.mapName as MapName)
        if (result.suggestedTeam) {
          applyTeamPick(result, result.suggestedTeam)
        }
      },
    })
  }

  function applyTeamPick(result: AnalyzeDemoResult, team: 'A' | 'B') {
    setSelectedTeam(team)
    const preview = team === 'A' ? result.teamA : result.teamB
    setOurScore(String(preview.ourScore))
    setOpponentScore(String(preview.opponentScore))
  }

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setSaved(null)
    addResult.mutate(
      {
        opponent,
        ourScore: ourScore === '' ? undefined : Number(ourScore),
        opponentScore: opponentScore === '' ? undefined : Number(opponentScore),
        mapName: mapName || undefined,
        demoUrl: demoUrl || undefined,
        notes: notes || undefined,
        playedAtUtc: new Date(playedAt || Date.now()).toISOString(),
        category,
        tournamentId: category === 'Tournament' ? tournamentId || undefined : undefined,
        leagueId: category === 'League' ? leagueId || undefined : undefined,
        demoRoundsPlayed: analysis?.roundsPlayed,
        demoPlayers: analysis?.players,
      },
      {
        onSuccess: (result) => {
          setOpponent('')
          setOurScore('')
          setOpponentScore('')
          setMapName('')
          setDemoUrl('')
          setNotes('')
          setPlayedAt('')
          setAnalysis(null)
          setSelectedTeam('')
          setDemoInputKey((key) => key + 1)
          setSaved(result)
        },
      },
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
      className="flex w-full max-w-xl flex-col gap-3 rounded-md border border-neutral-800 p-4"
    >
      <h2 className="font-medium">Dodaj wynik</h2>

      <div className="flex gap-3">
        <input
          required
          placeholder="Przeciwnik"
          value={opponent}
          onChange={(event) => setOpponent(event.target.value)}
          className={inputClass}
        />
        <input
          type="number"
          min={0}
          placeholder="Nasz wynik"
          value={ourScore}
          onChange={(event) => setOurScore(event.target.value)}
          className="w-28 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <input
          type="number"
          min={0}
          placeholder="Wynik przeciwnika"
          value={opponentScore}
          onChange={(event) => setOpponentScore(event.target.value)}
          className="w-28 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
      </div>

      <div className="flex gap-3">
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
        <TournamentPicker
          tournaments={tournaments ?? []}
          value={tournamentId}
          onChange={setTournamentId}
        />
      )}

      {category === 'League' && <LeaguePicker leagues={leagues ?? []} value={leagueId} onChange={setLeagueId} />}

      <label className="flex flex-col gap-1 text-sm text-neutral-400">
        Plik demki (opcjonalnie) — analiza wypełni mapę, wynik i staty graczy z SteamID64 na Waszym koncie
        <input
          type="file"
          accept=".dem"
          key={demoInputKey}
          onChange={handleDemoSelected}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm text-neutral-200 outline-none file:mr-3 file:rounded file:border-0 file:bg-neutral-800 file:px-3 file:py-1 file:text-sm file:text-neutral-200 focus:border-neutral-500"
        />
      </label>

      {analyzeDemo.isPending && (
        <p className="flex items-center gap-2 text-sm text-neutral-400">
          <span className="h-3.5 w-3.5 shrink-0 animate-spin rounded-full border-2 border-neutral-600 border-t-neutral-300" />
          Wgrywanie i analiza demki… ({elapsedSeconds}s) — duży plik może potrwać do minuty, proszę czekać.
        </p>
      )}

      {analyzeDemo.isError && (
        <p className="text-sm text-red-400">
          {analyzeDemo.error instanceof ApiError ? analyzeDemo.error.message : 'Nie udało się przeanalizować demki.'}
        </p>
      )}

      {analysis && (
        <div className="flex flex-col gap-2 rounded-md border border-neutral-800 p-3 text-sm">
          <p className="text-neutral-400">
            Rund w demce: {analysis.roundsPlayed}
            {analysis.mapName ? ` · Mapa: ${analysis.mapName}` : ''} — która drużyna to my?
          </p>
          <div className="flex flex-col gap-2 sm:flex-row">
            {(['A', 'B'] as const).map((team) => {
              const preview = team === 'A' ? analysis.teamA : analysis.teamB
              return (
                <label
                  key={team}
                  className={`flex-1 cursor-pointer rounded-md border px-3 py-2 transition ${
                    selectedTeam === team ? 'border-red-500 bg-red-950/30' : 'border-neutral-800 bg-neutral-900'
                  }`}
                >
                  <input
                    type="radio"
                    name="demoTeam"
                    className="sr-only"
                    checked={selectedTeam === team}
                    onChange={() => applyTeamPick(analysis, team)}
                  />
                  <span className="block text-neutral-200">
                    {preview.playerNames.join(', ')} — {preview.ourScore}:{preview.opponentScore}
                  </span>
                </label>
              )
            })}
          </div>
          <p className="text-xs text-neutral-500">
            Wybór wypełnia pola wyniku powyżej — możesz je jeszcze poprawić ręcznie przed zapisem.
          </p>
        </div>
      )}

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

      {addResult.isError && (
        <p className="text-sm text-red-400">
          {addResult.error instanceof ApiError ? addResult.error.message : 'Nie udało się dodać wyniku.'}
        </p>
      )}

      {saved && (
        <p className="text-sm text-green-400">
          Zapisano: {saved.mapName ?? 'bez mapy'}, {saved.ourScore}:{saved.opponentScore}
        </p>
      )}

      <button
        type="submit"
        disabled={addResult.isPending || analyzeDemo.isPending || !canSubmit}
        className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {addResult.isPending ? 'Dodawanie…' : 'Dodaj wynik'}
      </button>
    </form>
  )
}

interface TournamentPickerProps {
  tournaments: { id: string; name: string }[]
  value: string
  onChange: (tournamentId: string) => void
}

/** Selects an existing tournament to group this result under, or creates a new one inline. */
function TournamentPicker({ tournaments, value, onChange }: TournamentPickerProps) {
  const [newName, setNewName] = useState('')
  const createTournament = useCreateTournament()

  function handleCreate() {
    if (!newName.trim()) return
    createTournament.mutate(newName, {
      onSuccess: (tournament) => {
        onChange(tournament.id)
        setNewName('')
      },
    })
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

interface LeaguePickerProps {
  leagues: { id: string; name: string; season: string; type: LeagueType }[]
  value: string
  onChange: (leagueId: string) => void
}

/** Selects an existing league season to group this result under, or creates a new one inline. */
function LeaguePicker({ leagues, value, onChange }: LeaguePickerProps) {
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
