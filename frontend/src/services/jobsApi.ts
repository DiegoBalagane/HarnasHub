import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

/** Lifecycle state of a background job. */
export type JobStatus = 'Queued' | 'Running' | 'Succeeded' | 'Failed'

/** What every endpoint starting a long operation returns (202 Accepted). */
export interface JobAccepted {
  jobId: string
}

/** A background job; `result` has the shape the operation's endpoint used to return synchronously, set once it succeeded. */
export interface Job<TResult = unknown> {
  id: string
  kind: string
  status: JobStatus
  progress: number
  stage: string | null
  result: TResult | null
  error: string | null
  createdAtUtc: string
  startedAtUtc: string | null
  finishedAtUtc: string | null
}

/** True once the job can't change any more. */
export function isJobFinished(job: Pick<Job, 'status'> | undefined): boolean {
  return job?.status === 'Succeeded' || job?.status === 'Failed'
}

export const jobsApi = {
  getJob: <TResult>(jobId: string) => apiClient.get<Job<TResult>>(API_ENDPOINTS.jobs.byId(jobId)),
}
