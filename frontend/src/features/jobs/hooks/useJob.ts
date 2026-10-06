import { useQuery } from '@tanstack/react-query'
import { JOB_SETTINGS } from '../../../constants'
import { isJobFinished, jobsApi, type Job } from '../../../services/jobsApi'
import { isRealtimeConnected } from '../../../services/realtime'

/** Query key of one background job — the realtime topic `job:{id}` invalidates exactly this. */
export function jobKey(jobId: string | null) {
  return ['jobs', jobId]
}

/** Polling interval while the job runs: fast without a live connection, a slow safety net with one. */
export function jobPollInterval(job: Pick<Job, 'status'> | undefined, connected: boolean): number | false {
  if (isJobFinished(job)) return false
  return connected ? JOB_SETTINGS.connectedPollIntervalMs : JOB_SETTINGS.pollIntervalMs
}

/** State of one background job, kept fresh by realtime pushes with polling as a fallback; idle when `jobId` is null. */
export function useJob<TResult>(jobId: string | null) {
  const query = useQuery({
    queryKey: jobKey(jobId),
    queryFn: () => jobsApi.getJob<TResult>(jobId as string),
    enabled: jobId !== null,
    refetchInterval: (current) => jobPollInterval(current.state.data, isRealtimeConnected()),
  })

  const job = jobId === null ? undefined : query.data
  const isSucceeded = job?.status === 'Succeeded'
  const isFailed = job?.status === 'Failed' || (jobId !== null && query.isError)

  return {
    job,
    isActive: jobId !== null && !isSucceeded && !isFailed,
    isSucceeded,
    isFailed,
    result: isSucceeded ? (job.result as TResult) : undefined,
    error: isFailed ? (job?.error ?? query.error?.message ?? 'Nie udało się sprawdzić stanu zadania.') : null,
  }
}
