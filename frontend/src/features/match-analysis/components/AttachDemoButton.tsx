import { memo, useRef } from 'react'
import { JobProgress } from '../../jobs/components/JobProgress'
import { useAttachDemo } from '../hooks/useMatchAnalysis'

interface AttachDemoButtonProps {
  matchResultId: string
  /** Label shown on the button — "Dołącz demkę" for a match without a timeline, "Podmień demkę" otherwise. */
  label?: string
}

/** Lets a coach pick a .dem file, uploads it straight to object storage and attaches it as the match's timeline in the
 * background, showing the analysis progress inline. */
export const AttachDemoButton = memo(function AttachDemoButton({ matchResultId, label = 'Dołącz demkę' }: AttachDemoButtonProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const attachDemo = useAttachDemo(matchResultId)

  function handleFile(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (file) {
      attachDemo.start(file)
    }
  }

  return (
    <div className="flex flex-col gap-1">
      <input ref={inputRef} type="file" accept=".dem" className="hidden" onChange={handleFile} />
      <button
        type="button"
        disabled={attachDemo.isBusy}
        onClick={() => inputRef.current?.click()}
        className="self-start rounded-md border border-neutral-800 px-3 py-1.5 text-sm text-primary-400 transition hover:border-neutral-600 disabled:opacity-50"
      >
        {attachDemo.isBusy ? 'Analiza demki w toku…' : label}
      </button>
      <JobProgress
        job={attachDemo.job}
        isStarting={attachDemo.isStarting}
        startingLabel="Wgrywanie demki…"
        error={attachDemo.error}
      />
      {attachDemo.isSucceeded && attachDemo.result && !attachDemo.result.ourTeamResolved && (
        <p className="text-sm text-primary-300">
          Oś czasu zapisana, ale nie rozpoznano naszej drużyny — uzupełnij SteamID64 graczy w składzie.
        </p>
      )}
    </div>
  )
})
