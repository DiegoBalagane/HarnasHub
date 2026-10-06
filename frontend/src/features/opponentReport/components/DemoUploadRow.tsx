import { memo, useEffect } from 'react'
import type { OpponentDemo } from '../../../services/opponentDemosApi'
import { JobProgress } from '../../jobs/components/JobProgress'
import { useJob } from '../../jobs/hooks/useJob'
import type { DemoUploadItem } from '../hooks/useOpponentDemos'

/** Props of {@link DemoUploadRow}. */
interface DemoUploadRowProps {
  item: DemoUploadItem
  /** Called once when the file's analysis job succeeded. */
  onDone: (id: string, demo: OpponentDemo) => void
  /** Called once when the file's analysis job failed. */
  onFailed: (id: string, message: string) => void
}

const statusLabels: Record<DemoUploadItem['status'], string> = {
  queued: 'w kolejce',
  uploading: 'wgrywanie',
  analysing: 'analiza',
  done: 'gotowe',
  error: 'błąd',
}

/** One file of the opponent demo upload: upload progress, then its background analysis progress (per file). */
export const DemoUploadRow = memo(function DemoUploadRow({ item, onDone, onFailed }: DemoUploadRowProps) {
  const isFollowing = item.status === 'analysing' && item.jobId !== undefined
  const analysis = useJob<OpponentDemo>(isFollowing ? (item.jobId ?? null) : null)

  useEffect(() => {
    if (!isFollowing) return
    if (analysis.isSucceeded && analysis.result) onDone(item.id, analysis.result)
    else if (analysis.isFailed) onFailed(item.id, analysis.error ?? 'Błąd analizy demki.')
  }, [isFollowing, analysis.isSucceeded, analysis.isFailed, analysis.result, analysis.error, item.id, onDone, onFailed])

  return (
    <li className="flex flex-col gap-1 rounded-md bg-neutral-900 px-3 py-1.5">
      <div className="flex justify-between gap-3">
        <span className="truncate">{item.fileName}</span>
        <span className={item.status === 'error' ? 'text-danger-400' : 'text-neutral-400'}>
          {statusLabels[item.status]}
          {item.status === 'uploading' && ` ${Math.round(item.progress * 100)}%`}
        </span>
      </div>
      {item.status === 'uploading' && (
        <div className="h-1 rounded bg-neutral-800">
          <div className="h-1 rounded bg-primary-500" style={{ width: `${item.progress * 100}%` }} />
        </div>
      )}
      {isFollowing && (
        <JobProgress job={analysis.job} isStarting={!analysis.job} startingLabel="Analiza w kolejce…" showHint={false} />
      )}
      {item.message && (
        <span className={`text-xs ${item.status === 'error' ? 'text-danger-400' : 'text-warning-300'}`}>{item.message}</span>
      )}
    </li>
  )
})
