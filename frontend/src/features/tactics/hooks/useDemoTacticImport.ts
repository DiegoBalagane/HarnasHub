import { useMutation, useQueryClient } from '@tanstack/react-query'
import { resultsApi, uploadFileToPresignedUrl } from '../../../services/resultsApi'
import { tacticsApi, type DemoNades, type ImportTacticFromDemoPayload } from '../../../services/tacticsApi'
import { useBackgroundJob } from '../../jobs/hooks/useBackgroundJob'

/** Uploads a demo to object storage and starts the grenade extraction job. */
async function startExtraction(demoFile: File) {
  const { uploadUrl, objectKey } = await resultsApi.presignDemoUpload()
  await uploadFileToPresignedUrl(uploadUrl, demoFile)
  return tacticsApi.extractDemoNades(objectKey)
}

/** Uploads a demo straight to object storage (presigned URL) and parses every round's grenades out of it in the
 * background; the parsed rounds arrive via `onExtracted`. */
export function useExtractDemoNades(onExtracted: (demo: DemoNades) => void) {
  return useBackgroundJob<File, DemoNades>(startExtraction, { onSucceeded: onExtracted })
}

/** Saves the selected grenades as a new tactic (and optionally nade-library entries), refreshing both lists. */
export function useImportTacticFromDemo() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: ImportTacticFromDemoPayload) => tacticsApi.importFromDemo(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tactics'] })
      queryClient.invalidateQueries({ queryKey: ['nades'] })
    },
  })
}
