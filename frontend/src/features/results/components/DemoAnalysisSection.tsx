import { memo } from 'react'
import type { Job } from '../../../services/jobsApi'
import type { AnalyzeDemoResult, FaceitFactionPrefill } from '../../../services/resultsApi'
import { JobProgress } from '../../jobs/components/JobProgress'

/** Props of {@link DemoAnalysisSection}. */
interface DemoAnalysisSectionProps {
  /** Changing it remounts the file input (the only way to clear a chosen file). */
  inputKey: number
  onFileSelected: (file: File) => void
  job?: Pick<Job, 'status' | 'progress' | 'stage'>
  isStarting: boolean
  error: string | null
  analysis: AnalyzeDemoResult | null
  selectedTeam: 'A' | 'B' | ''
  onPickTeam: (team: 'A' | 'B') => void
  /** The opponent's FACEIT faction when the file name identified a FACEIT match and the opponent side is known. */
  opponentFaction: FaceitFactionPrefill | null
  linkOpponent: boolean
  onLinkOpponentChange: (value: boolean) => void
}

/** Demo step of the result form: file picker, background analysis progress, team pick and the FACEIT match recognised
 * from the file name (with the "link the opponent" option). */
export const DemoAnalysisSection = memo(function DemoAnalysisSection({
  inputKey,
  onFileSelected,
  job,
  isStarting,
  error,
  analysis,
  selectedTeam,
  onPickTeam,
  opponentFaction,
  linkOpponent,
  onLinkOpponentChange,
}: DemoAnalysisSectionProps) {
  const faceit = analysis?.faceitMatch ?? null

  return (
    <>
      <label className="flex flex-col gap-1 text-sm text-neutral-400">
        Plik demki (opcjonalnie) — analiza wypełni mapę, wynik i staty graczy z SteamID64 na Waszym koncie; oryginalna
        nazwa pliku z FACEIT uzupełni też rywala, datę i kategorię
        <input
          type="file"
          accept=".dem"
          key={inputKey}
          onChange={(event) => {
            const file = event.target.files?.[0]
            if (file) onFileSelected(file)
          }}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm text-neutral-200 outline-none file:mr-3 file:rounded file:border-0 file:bg-neutral-800 file:px-3 file:py-1 file:text-sm file:text-neutral-200 focus:border-neutral-500"
        />
      </label>

      <JobProgress job={job} isStarting={isStarting} startingLabel="Wgrywanie demki…" error={error} />

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
                    selectedTeam === team ? 'border-primary-500 bg-primary-950/30' : 'border-neutral-800 bg-neutral-900'
                  }`}
                >
                  <input
                    type="radio"
                    name="demoTeam"
                    className="sr-only"
                    checked={selectedTeam === team}
                    onChange={() => onPickTeam(team)}
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

          {faceit && (
            <p className="text-xs text-neutral-400">
              Rozpoznano mecz FACEIT{faceit.competitionName ? ` (${faceit.competitionName})` : ''} — uzupełniono
              {opponentFaction ? ' rywala,' : ''} datę i kategorię. Wszystko możesz zmienić.
            </p>
          )}
          {faceit && opponentFaction && (
            <label className="flex items-center gap-2 text-sm text-neutral-300">
              <input type="checkbox" checked={linkOpponent} onChange={(event) => onLinkOpponentChange(event.target.checked)} />
              Powiąż przeciwnika z FACEIT ({opponentFaction.nicknames.join(', ')})
            </label>
          )}
          {analysis.faceitNote && <p className="text-xs text-neutral-500">{analysis.faceitNote}</p>}
        </div>
      )}
    </>
  )
})
