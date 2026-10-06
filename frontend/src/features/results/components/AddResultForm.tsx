import { useEffect, useMemo, useRef, useState } from 'react'
import type { MapName } from '../../../services/nadesApi'
import type { AnalyzeDemoResult, FaceitMatchPrefill, MatchCategory, MatchResult } from '../../../services/resultsApi'
import { ApiError } from '../../../services/apiClient'
import { DateTimePicker } from '../../../components/ui/DateTimePicker'
import { useModalGuard } from '../../../components/ModalGuardContext'
import { mapNames } from '../../nades/labels'
import { useLinkOpponentFaceit } from '../../opponentReport/hooks/useOpponentReport'
import { OpponentNameInput } from '../../opponents/components/OpponentNameInput'
import { buildAnalysisPatch, linkSourceOf, opponentFaction, opponentNameOf, type TouchedField } from '../faceitPrefill'
import { useAddResult, useAnalyzeDemo } from '../hooks/useResults'
import { useLeagues } from '../hooks/useLeagues'
import { useTournaments } from '../hooks/useTournaments'
import { matchCategories, matchCategoryLabels } from '../labels'
import { DemoAnalysisSection } from './DemoAnalysisSection'
import { LeaguePicker } from './LeaguePicker'
import { TournamentPicker } from './TournamentPicker'

const inputClass =
  'flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

/** Coach/Manager-only form for logging a scrim/match/tournament result, grouped under a tournament or league when relevant.
 * A demo upload is a separate "analyse" step (a background job with inline progress), done before the result is submitted —
 * the map, score, a stat line per matched roster member and, for a FACEIT demo, the opponent/date/category are prefilled
 * from that preview, but every prefilled field stays freely editable, so whatever ends up in the form at submit time is
 * exactly what gets saved. */
interface AddResultFormProps {
  /** Called after saving, e.g. to close the modal hosting the form. */
  onDone?: () => void
}

export function AddResultForm({ onDone }: AddResultFormProps) {
  const [opponent, setOpponent] = useState('')
  const [ourScore, setOurScore] = useState('')
  const [opponentScore, setOpponentScore] = useState('')
  const [mapName, setMapName] = useState<MapName | ''>('')
  const [notes, setNotes] = useState('')
  const [playedAt, setPlayedAt] = useState('')
  const [category, setCategory] = useState<MatchCategory>('Scrimmage')
  const [tournamentId, setTournamentId] = useState('')
  const [leagueId, setLeagueId] = useState('')
  const [saved, setSaved] = useState<MatchResult | null>(null)
  // A file input keeps showing the chosen file name after its state is cleared — remounting it is the only way to reset it.
  const [demoInputKey, setDemoInputKey] = useState(0)
  const [analysis, setAnalysis] = useState<AnalyzeDemoResult | null>(null)
  const [selectedTeam, setSelectedTeam] = useState<'A' | 'B' | ''>('')
  const [linkOpponent, setLinkOpponent] = useState(false)
  // The opponent name last filled in from FACEIT — replaced on a new team pick only while the coach hasn't edited it.
  const prefilledOpponentRef = useRef('')
  // Fields the coach edited by hand — a demo analysis finishing later never overwrites these.
  const touchedRef = useRef<Set<TouchedField>>(new Set())
  const appliedResultRef = useRef<AnalyzeDemoResult | null>(null)

  const addResult = useAddResult()
  const linkFaceit = useLinkOpponentFaceit(opponent)
  const analyzeDemo = useAnalyzeDemo()
  const { data: tournaments } = useTournaments()
  const { data: leagues } = useLeagues()

  const faction = useMemo(
    () => (analysis?.faceitMatch ? opponentFaction(analysis.faceitMatch, selectedTeam) : null),
    [analysis, selectedTeam],
  )

  function touch(field: TouchedField) {
    touchedRef.current.add(field)
  }

  function handleAnalyzed(result: AnalyzeDemoResult) {
    setAnalysis(result)
    const patch = buildAnalysisPatch(result, touchedRef.current)
    if (patch.mapName) setMapName(patch.mapName as MapName)
    if (patch.selectedTeam) setSelectedTeam(patch.selectedTeam)
    if (patch.ourScore !== undefined) setOurScore(patch.ourScore)
    if (patch.opponentScore !== undefined) setOpponentScore(patch.opponentScore)
    if (patch.playedAt) setPlayedAt(patch.playedAt)
    if (patch.category && patch.category !== category) {
      setCategory(patch.category)
      setTournamentId('')
      setLeagueId('')
    }
    if (result.faceitMatch) applyFaceitOpponent(result.faceitMatch, result.suggestedTeam ?? '')
  }

  // The result arrives through the slotted job — also when the form was closed meanwhile and is reopened later.
  useEffect(() => {
    const result = analyzeDemo.result
    if (!result || appliedResultRef.current === result) return
    appliedResultRef.current = result
    handleAnalyzed(result)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [analyzeDemo.result])

  function applyTeamPick(result: AnalyzeDemoResult, team: 'A' | 'B') {
    setSelectedTeam(team)
    const preview = team === 'A' ? result.teamA : result.teamB
    setOurScore(String(preview.ourScore))
    setOpponentScore(String(preview.opponentScore))
  }

  function applyFaceitOpponent(prefill: FaceitMatchPrefill, team: 'A' | 'B' | '') {
    const theirs = opponentFaction(prefill, team)
    if (!theirs) return
    const name = opponentNameOf(theirs)
    const previous = prefilledOpponentRef.current
    setOpponent((current) => (current === '' || current === previous ? name : current))
    prefilledOpponentRef.current = name
    setLinkOpponent(theirs.linkedOpponentName === null)
  }

  function handlePickTeam(team: 'A' | 'B') {
    if (!analysis) return
    touchedRef.current.delete('score')
    applyTeamPick(analysis, team)
    if (analysis.faceitMatch) applyFaceitOpponent(analysis.faceitMatch, team)
  }

  function handleOpponentChange(value: string) {
    touch('opponent')
    setOpponent(value)
  }

  function handleDemoSelected(file: File) {
    setAnalysis(null)
    setSelectedTeam('')
    analyzeDemo.start(file)
  }

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setSaved(null)
    const linkSource = linkOpponent && faction ? linkSourceOf(faction) : null
    addResult.mutate(
      {
        opponent,
        ourScore: ourScore === '' ? undefined : Number(ourScore),
        opponentScore: opponentScore === '' ? undefined : Number(opponentScore),
        mapName: mapName || undefined,
        notes: notes || undefined,
        playedAtUtc: new Date(playedAt || Date.now()).toISOString(),
        category,
        tournamentId: category === 'Tournament' ? tournamentId || undefined : undefined,
        leagueId: category === 'League' ? leagueId || undefined : undefined,
        demoRoundsPlayed: analysis?.roundsPlayed,
        demoPlayers: analysis?.players,
        ourTeamSteamIds: selectedTeam && analysis ? (selectedTeam === 'A' ? analysis.teamA : analysis.teamB).steamIds : undefined,
        pendingTimelineKey: analysis?.pendingTimelineKey ?? undefined,
      },
      {
        onSuccess: (result) => {
          // Runs as a background job (link + first FACEIT sync); its progress shows on the opponent's report page.
          if (linkSource) linkFaceit.start(linkSource)
          setOpponent('')
          setOurScore('')
          setOpponentScore('')
          setMapName('')
          setNotes('')
          setPlayedAt('')
          setAnalysis(null)
          setSelectedTeam('')
          setLinkOpponent(false)
          prefilledOpponentRef.current = ''
          setDemoInputKey((key) => key + 1)
          touchedRef.current.clear()
          analyzeDemo.reset()
          setSaved(result)
          onDone?.()
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

  useModalGuard({
    isBusy: analyzeDemo.isBusy || addResult.isPending,
    isDirty: [opponent, ourScore, opponentScore, mapName, notes, playedAt].some((value) => value !== '') || analysis !== null,
    onDiscard: analyzeDemo.reset,
  })

  return (
    <form
      onSubmit={handleSubmit}
      className="flex w-full max-w-xl flex-col gap-3 rounded-md border border-neutral-800 p-4"
    >
      <h2 className="font-medium">Dodaj wynik</h2>

      <div className="flex gap-3">
        <OpponentNameInput required placeholder="Przeciwnik" value={opponent} onChange={handleOpponentChange} className={inputClass} />
        <input
          type="number"
          min={0}
          placeholder="Nasz wynik"
          value={ourScore}
          onChange={(event) => {
            touch('score')
            setOurScore(event.target.value)
          }}
          className="w-28 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <input
          type="number"
          min={0}
          placeholder="Wynik przeciwnika"
          value={opponentScore}
          onChange={(event) => {
            touch('score')
            setOpponentScore(event.target.value)
          }}
          className="w-28 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
      </div>

      <div className="flex gap-3">
        <select
          value={category}
          onChange={(event) => {
            touch('category')
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
          onChange={(event) => {
            touch('map')
            setMapName(event.target.value as MapName | '')
          }}
          className={inputClass}
        >
          <option value="">Mapa (opcjonalnie)</option>
          {mapNames.map((map) => (
            <option key={map} value={map}>
              {map}
            </option>
          ))}
        </select>
        <DateTimePicker
          label="Data i godzina meczu"
          placeholder="Data i godzina"
          value={playedAt}
          onChange={(value) => {
            touch('playedAt')
            setPlayedAt(value)
          }}
          className={inputClass}
        />
      </div>

      {category === 'Tournament' && (
        <TournamentPicker tournaments={tournaments ?? []} value={tournamentId} onChange={setTournamentId} />
      )}

      {category === 'League' && <LeaguePicker leagues={leagues ?? []} value={leagueId} onChange={setLeagueId} />}

      <DemoAnalysisSection
        inputKey={demoInputKey}
        onFileSelected={handleDemoSelected}
        job={analyzeDemo.job}
        isStarting={analyzeDemo.isStarting}
        error={analyzeDemo.error}
        analysis={analysis}
        selectedTeam={selectedTeam}
        onPickTeam={handlePickTeam}
        opponentFaction={faction}
        linkOpponent={linkOpponent}
        onLinkOpponentChange={setLinkOpponent}
      />

      <textarea
        placeholder="Notatki pomeczowe (opcjonalnie)"
        value={notes}
        onChange={(event) => setNotes(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {addResult.isError && (
        <p className="text-sm text-danger-400">
          {addResult.error instanceof ApiError ? addResult.error.message : 'Nie udało się dodać wyniku.'}
        </p>
      )}

      {saved && (
        <p className="text-sm text-success-400">
          Zapisano: {saved.mapName ?? 'bez mapy'}, {saved.ourScore}:{saved.opponentScore}
        </p>
      )}

      <button
        type="submit"
        disabled={addResult.isPending || analyzeDemo.isBusy || !canSubmit}
        className="self-start rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
      >
        {addResult.isPending ? 'Dodawanie…' : 'Dodaj wynik'}
      </button>
    </form>
  )
}
