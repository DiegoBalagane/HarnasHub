import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { act, renderHook, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { JOB_SETTINGS } from '../../../constants'
import type { Job } from '../../../services/jobsApi'
import { mockFetch } from '../../../test/renderWithProviders'
import { useJobStore } from '../stores/useJobStore'
import { useBackgroundJob } from './useBackgroundJob'
import { jobPollInterval, useJob } from './useJob'

function wrapper({ children }: { children: ReactNode }) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false, gcTime: 0 }, mutations: { retry: false } } })
  return <QueryClientProvider client={client}>{children}</QueryClientProvider>
}

function job(overrides: Partial<Job<{ rounds: number }>> = {}): Job<{ rounds: number }> {
  return {
    id: 'job-1',
    kind: 'results.analyze-stored-demo',
    status: 'Running',
    progress: 40,
    stage: 'Analiza demki',
    result: null,
    error: null,
    createdAtUtc: '2026-10-06T12:00:00Z',
    startedAtUtc: '2026-10-06T12:00:01Z',
    finishedAtUtc: null,
    ...overrides,
  }
}

describe('jobPollInterval', () => {
  it('polls fast without realtime, slowly with it and stops once finished', () => {
    expect(jobPollInterval({ status: 'Running' }, false)).toBe(JOB_SETTINGS.pollIntervalMs)
    expect(jobPollInterval({ status: 'Queued' }, true)).toBe(JOB_SETTINGS.connectedPollIntervalMs)
    expect(jobPollInterval({ status: 'Succeeded' }, false)).toBe(false)
    expect(jobPollInterval({ status: 'Failed' }, true)).toBe(false)
  })
})

describe('useJob', () => {
  it('stays idle without a job id', () => {
    const fetchMock = mockFetch(job())
    const { result } = renderHook(() => useJob(null), { wrapper })

    expect(result.current.isActive).toBe(false)
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('exposes the result of a succeeded job', async () => {
    mockFetch(job({ status: 'Succeeded', progress: 100, result: { rounds: 24 } }))
    const { result } = renderHook(() => useJob<{ rounds: number }>('job-1'), { wrapper })

    await waitFor(() => expect(result.current.isSucceeded).toBe(true))
    expect(result.current.result).toEqual({ rounds: 24 })
    expect(result.current.isActive).toBe(false)
  })

  it('exposes the polish error of a failed job', async () => {
    mockFetch(job({ status: 'Failed', error: 'Nie udało się odczytać demki.' }))
    const { result } = renderHook(() => useJob('job-1'), { wrapper })

    await waitFor(() => expect(result.current.isFailed).toBe(true))
    expect(result.current.error).toBe('Nie udało się odczytać demki.')
  })
})

describe('useBackgroundJob', () => {
  it('starts the job, follows it and calls onSucceeded exactly once', async () => {
    useJobStore.setState({ jobsBySlot: {}, handledJobIds: {} })
    mockFetch(job({ id: 'job-7', status: 'Succeeded', progress: 100, result: { rounds: 13 } }))
    const start = vi.fn(async (_file: string) => ({ jobId: 'job-7' }))
    const onSucceeded = vi.fn()
    const { result, rerender } = renderHook(() => useBackgroundJob<string, { rounds: number }>(start, { onSucceeded }), {
      wrapper,
    })

    act(() => result.current.start('demo.dem'))

    await waitFor(() => expect(onSucceeded).toHaveBeenCalledWith({ rounds: 13 }))
    rerender()
    expect(onSucceeded).toHaveBeenCalledTimes(1)
    expect(start.mock.calls[0]?.[0]).toBe('demo.dem')
    expect(result.current.isBusy).toBe(false)
  })

  it('keeps a slotted job in the session store', async () => {
    useJobStore.setState({ jobsBySlot: {}, handledJobIds: {} })
    mockFetch(job({ id: 'job-9' }))
    const { result } = renderHook(
      () => useBackgroundJob<void, unknown>(async () => ({ jobId: 'job-9' }), { slot: 'faceit-refresh:rivals' }),
      { wrapper },
    )

    act(() => result.current.start())

    await waitFor(() => expect(useJobStore.getState().jobsBySlot['faceit-refresh:rivals']).toBe('job-9'))
    await waitFor(() => expect(result.current.isRunning).toBe(true))
  })

  it('reports a failed start request', async () => {
    const { result } = renderHook(
      () => useBackgroundJob<void, unknown>(async () => Promise.reject(new Error('Brak uprawnień.'))),
      { wrapper },
    )

    act(() => result.current.start())

    await waitFor(() => expect(result.current.error).toBe('Brak uprawnień.'))
  })
})
