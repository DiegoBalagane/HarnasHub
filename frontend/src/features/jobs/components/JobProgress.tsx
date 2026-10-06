import { memo } from 'react'
import type { Job } from '../../../services/jobsApi'

/** Props of {@link JobProgress}. */
interface JobProgressProps {
  /** The tracked job (undefined until the start request returned). */
  job?: Pick<Job, 'status' | 'progress' | 'stage'>
  /** True while the file is uploaded / the job is being started. */
  isStarting?: boolean
  /** Label shown while starting, e.g. "Wgrywanie demki…". */
  startingLabel?: string
  /** Polish error to show instead of the bar. */
  error?: string | null
  /** Whether to add the "you can keep using the app" hint (off for compact lists). */
  showHint?: boolean
}

/** Inline progress of a background job: an indeterminate bar while starting, then percentage + stage; nothing once done. */
export const JobProgress = memo(function JobProgress({
  job,
  isStarting = false,
  startingLabel = 'Uruchamianie…',
  error,
  showHint = true,
}: JobProgressProps) {
  if (error) {
    return (
      <p role="alert" className="text-sm text-danger-400">
        {error}
      </p>
    )
  }

  const isRunning = job !== undefined && (job.status === 'Queued' || job.status === 'Running')
  if (!isStarting && !isRunning) return null

  const percent = isRunning ? Math.max(0, Math.min(100, job.progress)) : 0
  const label = isRunning ? (job.stage ?? (job.status === 'Queued' ? 'W kolejce' : 'Przetwarzanie')) : startingLabel

  return (
    <div className="flex flex-col gap-1 text-sm text-neutral-400">
      <div className="flex justify-between gap-3">
        <span>{label}</span>
        {isRunning && <span>{percent}%</span>}
      </div>
      <div
        role="progressbar"
        aria-label={label}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-valuenow={isRunning ? percent : undefined}
        className="h-1.5 overflow-hidden rounded bg-neutral-800"
      >
        <div
          className={`h-full rounded bg-primary-500 transition-[width] duration-500 ${isRunning ? '' : 'w-1/3 animate-pulse'}`}
          style={isRunning ? { width: `${percent}%` } : undefined}
        />
      </div>
      {showHint && (
        <span className="text-xs text-neutral-500">Możesz w tym czasie korzystać z aplikacji — wynik pojawi się tutaj.</span>
      )}
    </div>
  )
})
