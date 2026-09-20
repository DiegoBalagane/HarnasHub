import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { resultsApi, type AddResultPayload } from '../../../services/resultsApi'

/** Fetches every logged result, most recent first. */
export function useResults() {
  return useQuery({
    queryKey: ['results'],
    queryFn: resultsApi.getResults,
  })
}

/** Logs a new result and refreshes the results list. */
export function useAddResult() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: AddResultPayload) => resultsApi.addResult(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['results'] })
    },
  })
}

/** Parses an uploaded demo into a map/score/team-split preview — nothing is saved until addResult is submitted. */
export function useAnalyzeDemo() {
  return useMutation({
    mutationFn: (demoFile: File) => resultsApi.analyzeDemo(demoFile),
  })
}

/** Deletes a logged result (and its stat lines) and refreshes the results list. */
export function useDeleteResult() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (matchResultId: string) => resultsApi.deleteResult(matchResultId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['results'] })
    },
  })
}
