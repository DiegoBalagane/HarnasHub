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
