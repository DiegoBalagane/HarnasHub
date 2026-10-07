import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useCallback } from 'react'
import { ApiError } from '../../../services/apiClient'
import { matchAnalysisApi, type MatchDemoAnalysis } from '../../../services/matchAnalysisApi'
import { resultsApi, uploadFileToPresignedUrl } from '../../../services/resultsApi'
import { useBackgroundJob } from '../../jobs/hooks/useBackgroundJob'

/** True for the 404 the API returns when a match simply has no timeline yet — an expected state, not a failure. */
export function isMissingTimeline(error: unknown): boolean {
  return error instanceof ApiError && error.status === 404
}

function retryUnlessMissing(failureCount: number, error: Error) {
  return !isMissingTimeline(error) && failureCount < 2
}

/** Fetches a match's round-by-round timeline (404 = no demo attached yet, see isMissingTimeline). */
export function useMatchTimeline(matchResultId: string) {
  return useQuery({
    queryKey: ['match-analysis', matchResultId, 'timeline'],
    queryFn: () => matchAnalysisApi.getTimeline(matchResultId),
    retry: retryUnlessMissing,
  })
}

/** Fetches the automatic insights computed from a match's timeline. */
export function useMatchInsights(matchResultId: string) {
  return useQuery({
    queryKey: ['match-analysis', matchResultId, 'insights'],
    queryFn: () => matchAnalysisApi.getInsights(matchResultId),
    retry: retryUnlessMissing,
  })
}

/** Uploads a demo straight to object storage and attaches it to an existing match as a background job (slotted per
 * match, so the progress survives leaving the page), then refreshes its timeline. */
export function useAttachDemo(matchResultId: string) {
  const queryClient = useQueryClient()

  const start = useCallback(
    async (demoFile: File) => {
      const { uploadUrl, objectKey } = await resultsApi.presignDemoUpload()
      await uploadFileToPresignedUrl(uploadUrl, demoFile)
      return matchAnalysisApi.attachDemo(matchResultId, objectKey)
    },
    [matchResultId],
  )
  const onSucceeded = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['match-analysis', matchResultId] })
    queryClient.invalidateQueries({ queryKey: ['tactics', 'match-matches', matchResultId] })
    queryClient.invalidateQueries({ queryKey: ['round-replay', 'Match', matchResultId] })
  }, [queryClient, matchResultId])

  return useBackgroundJob<File, MatchDemoAnalysis>(start, { onSucceeded, slot: `attach-demo:${matchResultId}` })
}

/** Fetches the deep analysis (trades, clutches, openings, flashes, grenades vs library) of a match (404 = no demo attached yet). */
export function useMatchDeepAnalysis(matchResultId: string) {
  return useQuery({
    queryKey: ['match-analysis', matchResultId, 'analysis'],
    queryFn: () => matchAnalysisApi.getAnalysis(matchResultId),
    retry: retryUnlessMissing,
  })
}

/** Fetches multi-match aggregates for one map (Playbook "Statystyki"). */
export function useMapAnalytics(mapName: string | undefined) {
  return useQuery({
    queryKey: ['map-analytics', mapName],
    queryFn: () => matchAnalysisApi.getMapAnalytics(mapName as string),
    enabled: Boolean(mapName),
  })
}

/** Excludes / restores a player in one match's analysis (Coach/Manager) and refreshes everything derived from the timeline. */
export function useAnalysisPlayerExclusion(matchResultId: string) {
  const queryClient = useQueryClient()
  const refresh = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['match-analysis', matchResultId] })
    queryClient.invalidateQueries({ queryKey: ['tactics', 'match-matches', matchResultId] })
    queryClient.invalidateQueries({ queryKey: ['round-replay', 'Match', matchResultId] })
    queryClient.invalidateQueries({ queryKey: ['map-analytics'] })
    queryClient.invalidateQueries({ queryKey: ['stats'] })
  }, [queryClient, matchResultId])

  const exclude = useMutation({
    mutationFn: (steamId64: string) => matchAnalysisApi.excludePlayer(matchResultId, steamId64),
    onSuccess: refresh,
  })
  const include = useMutation({
    mutationFn: (steamId64: string) => matchAnalysisApi.includePlayer(matchResultId, steamId64),
    onSuccess: refresh,
  })

  return { exclude, include }
}
