import { useMutation } from '@tanstack/react-query'
import { useCallback, useEffect, useRef, useState } from 'react'
import type { JobAccepted } from '../../../services/jobsApi'
import { useJobStore } from '../stores/useJobStore'
import { useJob } from './useJob'

/** Options of {@link useBackgroundJob}. */
interface BackgroundJobOptions<TResult> {
  /** Called once per job when it succeeds (also when it finished while no component was following it). */
  onSucceeded?: (result: TResult) => void
  /** Keeps the job id in the session store under this key, so a page left and re-opened picks the job up again and
   * every component using the same slot shows the same job. Without it the job id lives in component state. */
  slot?: string
}

/** Starts a long operation (`start` uploads whatever it needs and returns the job id) and follows it to the end:
 * `isStarting` covers the upload/start request, `isRunning` the background job; the result arrives via `onSucceeded`. */
export function useBackgroundJob<TVariables, TResult>(
  start: (variables: TVariables) => Promise<JobAccepted>,
  { onSucceeded, slot }: BackgroundJobOptions<TResult> = {},
) {
  const [localJobId, setLocalJobId] = useState<string | null>(null)
  const slotJobId = useJobStore((state) => (slot ? (state.jobsBySlot[slot] ?? null) : null))
  const setSlotJob = useJobStore((state) => state.setSlotJob)
  const markHandled = useJobStore((state) => state.markHandled)
  const jobId = slot ? slotJobId : localJobId
  const onSucceededRef = useRef(onSucceeded)

  useEffect(() => {
    onSucceededRef.current = onSucceeded
  }, [onSucceeded])

  const setJobId = useCallback(
    (id: string | null) => (slot ? setSlotJob(slot, id) : setLocalJobId(id)),
    [slot, setSlotJob],
  )

  const startMutation = useMutation({
    mutationFn: start,
    onSuccess: (accepted) => setJobId(accepted.jobId),
  })
  const tracked = useJob<TResult>(jobId)

  useEffect(() => {
    if (jobId === null || !tracked.isSucceeded || useJobStore.getState().handledJobIds[jobId]) return
    markHandled(jobId)
    onSucceededRef.current?.(tracked.result as TResult)
  }, [jobId, tracked.isSucceeded, tracked.result, markHandled])

  const { mutate, reset: resetMutation } = startMutation
  const run = useCallback(
    (variables: TVariables) => {
      setJobId(null)
      mutate(variables)
    },
    [mutate, setJobId],
  )
  const reset = useCallback(() => {
    setJobId(null)
    resetMutation()
  }, [resetMutation, setJobId])

  const isStarting = startMutation.isPending
  return {
    start: run,
    reset,
    job: tracked.job,
    result: tracked.result,
    isStarting,
    isRunning: tracked.isActive,
    isBusy: isStarting || tracked.isActive,
    isSucceeded: tracked.isSucceeded,
    error: startMutation.error?.message ?? tracked.error,
  }
}
