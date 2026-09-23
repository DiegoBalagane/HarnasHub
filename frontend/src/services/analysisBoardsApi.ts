import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { MapName } from './nadesApi'
import { uploadFileToPresignedUrl } from './resultsApi'

/** A saved freehand drawing over either the built-in radar for `mapName` or an uploaded screenshot.
 * `strokesJson` is the raw stroke array — parsed by the canvas, never inspected by API code. */
export interface AnalysisBoard {
  id: string
  mapName: MapName
  title: string
  /** The raw storage key — round-trip it unchanged on an edit that doesn't touch the background, since sending
   * null there would be indistinguishable from "clear it". */
  backgroundImageObjectKey: string | null
  /** Presigned, time-limited URL resolved from backgroundImageObjectKey — null draws over the built-in radar instead. */
  backgroundImageUrl: string | null
  strokesJson: string
  updatedAtUtc: string
}

export interface SaveBoardPayload {
  mapName: MapName
  title: string
  backgroundImageObjectKey?: string | null
  strokesJson: string
}

interface PresignedBoardImageUpload {
  uploadUrl: string
  objectKey: string
}

export const analysisBoardsApi = {
  getBoards: (mapName?: MapName) => apiClient.get<AnalysisBoard[]>(API_ENDPOINTS.analysisBoards.list(mapName)),
  createBoard: (payload: SaveBoardPayload) => apiClient.post<AnalysisBoard>(API_ENDPOINTS.analysisBoards.list(), payload),
  updateBoard: (boardId: string, payload: Omit<SaveBoardPayload, 'mapName'>) =>
    apiClient.patch<AnalysisBoard>(API_ENDPOINTS.analysisBoards.byId(boardId), payload),
  deleteBoard: (boardId: string) => apiClient.delete<void>(API_ENDPOINTS.analysisBoards.byId(boardId)),
  presignImageUpload: () => apiClient.post<PresignedBoardImageUpload>(API_ENDPOINTS.analysisBoards.presignImageUpload, {}),
  /** Uploads a background image blob (pasted or picked) straight to object storage and returns its object key. */
  uploadBackgroundImage: async (file: Blob): Promise<string> => {
    const { uploadUrl, objectKey } = await analysisBoardsApi.presignImageUpload()
    await uploadFileToPresignedUrl(uploadUrl, file)
    return objectKey
  },
}
