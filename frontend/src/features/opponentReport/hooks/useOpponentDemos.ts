import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useCallback, useState } from 'react'
import type { MapName } from '../../../services/nadesApi'
import {
  opponentDemosApi,
  type DemoTeam,
  type OpponentDemo,
  type OpponentDemoDownloadResult,
} from '../../../services/opponentDemosApi'
import { useBackgroundJob } from '../../jobs/hooks/useBackgroundJob'
import { reportKey } from './useOpponentReport'

/** Query key of one opponent's demo list. */
function demosKey(name: string) {
  return ['opponent-demos', name.trim().toLowerCase()]
}

/** Refetches the demo list and the report (its tendencies come from the demos). */
function useInvalidateDemos(name: string) {
  const queryClient = useQueryClient()
  return useCallback(() => {
    queryClient.invalidateQueries({ queryKey: demosKey(name) })
    queryClient.invalidateQueries({ queryKey: reportKey(name) })
  }, [queryClient, name])
}

/** Lists the opponent's analysed demos with what the section may offer (uploads, FACEIT download). */
export function useOpponentDemos(name: string) {
  return useQuery({
    queryKey: demosKey(name),
    queryFn: () => opponentDemosApi.getDemos(name),
    enabled: name.trim() !== '',
  })
}

/** State of one file in the upload queue; `jobId` is its background analysis once the upload finished. */
export interface DemoUploadItem {
  id: string
  fileName: string
  status: 'queued' | 'uploading' | 'analysing' | 'done' | 'error'
  progress: number
  jobId?: string
  message?: string
}

/** Uploads several demos one after another (each: presign → PUT with progress → start its analysis job) without waiting
 * for the analyses — they run in the background while the next file uploads; rows follow their job and report back via
 * `complete`/`fail`. */
export function useUploadOpponentDemos(name: string) {
  const invalidate = useInvalidateDemos(name)
  const [items, setItems] = useState<DemoUploadItem[]>([])
  const [busy, setBusy] = useState(false)

  const update = useCallback((id: string, patch: Partial<DemoUploadItem>) => {
    setItems((current) => current.map((item) => (item.id === id ? { ...item, ...patch } : item)))
  }, [])

  const upload = useCallback(
    async (files: File[]) => {
      const queued = files.map((file) => ({
        id: crypto.randomUUID(),
        fileName: file.name,
        status: 'queued' as const,
        progress: 0,
      }))
      setItems((current) => [...current.filter((item) => item.status !== 'done'), ...queued])
      setBusy(true)

      for (const [index, file] of files.entries()) {
        const id = queued[index].id
        try {
          update(id, { status: 'uploading' })
          const { jobId } = await opponentDemosApi.uploadAndAnalyze(name, file, (progress) =>
            update(id, { progress, status: progress >= 1 ? 'analysing' : 'uploading' }),
          )
          update(id, { status: 'analysing', progress: 1, jobId })
        } catch (error) {
          update(id, {
            status: 'error',
            message: error instanceof Error ? error.message : 'Błąd analizy demki.',
          })
        }
      }

      setBusy(false)
    },
    [name, update],
  )

  const complete = useCallback(
    (id: string, demo: OpponentDemo) => {
      update(id, {
        status: 'done',
        message: demo.teamResolved ? undefined : 'Nie rozpoznano drużyny rywala — wybierz ją na liście.',
      })
      invalidate()
    },
    [update, invalidate],
  )
  const fail = useCallback((id: string, message: string) => update(id, { status: 'error', message }), [update])

  return { items, busy, upload, complete, fail }
}

/** Sets which team of a demo is the opponent (fallback when automatic detection failed). */
export function useSetOpponentDemoTeam(name: string) {
  const invalidate = useInvalidateDemos(name)
  return useMutation({
    mutationFn: ({ demoId, team }: { demoId: string; team: DemoTeam }) =>
      opponentDemosApi.setTeam(demoId, team),
    onSuccess: invalidate,
  })
}

/** Deletes an analysed demo and its stored timeline. */
export function useDeleteOpponentDemo(name: string) {
  const invalidate = useInvalidateDemos(name)
  return useMutation({
    mutationFn: (demoId: string) => opponentDemosApi.deleteDemo(demoId),
    onSuccess: invalidate,
  })
}

/** Downloads and analyses the opponent's latest FACEIT team games (Downloads API) as a background job that can take
 * minutes — slotted per opponent, so leaving and re-opening the report keeps showing its progress. */
export function useFaceitDemoDownload(name: string) {
  const invalidate = useInvalidateDemos(name)
  const start = useCallback(
    ({ maps, count }: { maps: MapName[]; count: number }) => opponentDemosApi.faceitDownload(name, maps, count),
    [name],
  )
  return useBackgroundJob<{ maps: MapName[]; count: number }, OpponentDemoDownloadResult>(start, {
    onSucceeded: invalidate,
    slot: `faceit-download:${name.trim().toLowerCase()}`,
  })
}
