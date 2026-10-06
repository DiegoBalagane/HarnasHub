import { useState } from 'react'
import { useModalGuard } from '../../../../components/ModalGuardContext'
import type { DemoNades } from '../../../../services/tacticsApi'
import { JobProgress } from '../../../jobs/components/JobProgress'
import { useExtractDemoNades } from '../../hooks/useDemoTacticImport'

interface DemoUploadStepProps {
  /** Called with every round's grenades once the demo has been parsed. */
  onExtracted: (demo: DemoNades) => void
}

/** Wizard step 1: pick a .dem file, upload it to storage and parse its grenades in the background (progress inline). */
export function DemoUploadStep({ onExtracted }: DemoUploadStepProps) {
  const [file, setFile] = useState<File | null>(null)
  const extract = useExtractDemoNades(onExtracted)
  useModalGuard({ isBusy: extract.isBusy, isDirty: file !== null })

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    if (file) extract.start(file)
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-3">
      <p className="text-sm text-neutral-400">
        Wgraj demkę (.dem) — po analizie wybierzesz rundę, stronę i granaty, które trafią do nowej taktyki.
      </p>

      <input
        type="file"
        accept=".dem"
        onChange={(event) => setFile(event.target.files?.[0] ?? null)}
        className="text-sm text-neutral-300 file:mr-3 file:rounded-md file:border-0 file:bg-neutral-800 file:px-3 file:py-2 file:text-neutral-100"
      />

      <JobProgress job={extract.job} isStarting={extract.isStarting} startingLabel="Wgrywanie demki…" error={extract.error} />

      <button
        type="submit"
        disabled={!file || extract.isBusy}
        className="self-start rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
      >
        {extract.isBusy ? 'Analiza…' : 'Analizuj demkę'}
      </button>
    </form>
  )
}
