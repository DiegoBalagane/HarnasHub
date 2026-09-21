import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { DEMO_DIRECT_UPLOAD_MAX_BYTES } from '../../../constants'
import { resultsApi, uploadFileToPresignedUrl, type AddResultPayload } from '../../../services/resultsApi'

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

/** Parses an uploaded demo into a map/score/team-split preview — nothing is saved until addResult is submitted.
 * Demos over DEMO_DIRECT_UPLOAD_MAX_BYTES go through object storage instead of this app's own server, since the
 * hosting platform's edge proxy rejects large request bodies well before our own (much higher) server-side limit. */
export function useAnalyzeDemo() {
  return useMutation({
    mutationFn: async (demoFile: File) => {
      if (demoFile.size <= DEMO_DIRECT_UPLOAD_MAX_BYTES) {
        return resultsApi.analyzeDemo(demoFile)
      }

      const { uploadUrl, objectKey } = await resultsApi.presignDemoUpload()
      await uploadFileToPresignedUrl(uploadUrl, demoFile)
      return resultsApi.analyzeDemoFromStorage(objectKey)
    },
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
