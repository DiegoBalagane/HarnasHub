import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { JobAccepted } from './jobsApi'
import type { LeagueType } from './leaguesApi'
import type { DeathPosition } from './statsApi'

export type MatchCategory = 'Scrimmage' | 'League' | 'Tournament'

export interface MatchResult {
  id: string
  opponent: string
  ourScore: number
  opponentScore: number
  mapName: string | null
  demoUrl: string | null
  notes: string | null
  playedAtUtc: string
  category: MatchCategory
  tournamentId: string | null
  tournamentName: string | null
  leagueId: string | null
  leagueName: string | null
  leagueSeason: string | null
  leagueType: LeagueType | null
}

/** One demo participant's raw totals, as returned by analyzeDemo — round-tripped back on addResult so a stat line
 * can be saved without re-uploading the (possibly 100-300MB) demo file a second time. */
export interface AnalyzedDemoPlayer {
  steamId64: string
  demoPlayerName: string
  kills: number
  deaths: number
  assists: number
  headshots: number
  damageDealt: number
  entryKills: number
  entryDeaths: number
  kastRounds: number
  utilityDamage: number
  flashAssists: number
  multiKill2K: number
  multiKill3K: number
  multiKill4K: number
  multiKill5K: number
  deathPositions: DeathPosition[]
}

/** One of the two groups a demo's round-1 sides split into, with the score it would produce if this is "our" team. */
export interface DemoTeamPreview {
  playerNames: string[]
  steamIds: string[]
  ourScore: number
  opponentScore: number
}

export interface AnalyzeDemoResult {
  roundsPlayed: number
  mapName: string | null
  teamA: DemoTeamPreview
  teamB: DemoTeamPreview
  /** 'A' | 'B' when the roster's SteamID64s overlap one of the two groups — null when nobody matched. */
  suggestedTeam: 'A' | 'B' | null
  players: AnalyzedDemoPlayer[]
  /** Full match timeline parked in storage until addResult claims it — null when storage isn't configured. */
  pendingTimelineKey: string | null
  /** Prefill from the FACEIT match the file name points at — null for other names, without an API key or on FACEIT errors. */
  faceitMatch?: FaceitMatchPrefill | null
  /** Subtle explanation why a FACEIT-named demo got no prefill. */
  faceitNote?: string | null
}

/** One faction of a recognised FACEIT match; `demoTeam` is the demo team ('A' started T) it played as, when known. */
export interface FaceitFactionPrefill {
  name: string
  demoTeam: 'A' | 'B' | null
  playerIds: string[]
  nicknames: string[]
  /** Display name of an opponent already linked to this roster. */
  linkedOpponentName: string | null
}

/** Editable form prefill from a FACEIT match recognised from the demo file name. */
export interface FaceitMatchPrefill {
  matchId: string
  competitionName: string | null
  category: MatchCategory
  playedAtUtc: string | null
  mapName: string | null
  /** Index of our faction in `factions`, null when our roster wasn't found in the room. */
  ourFactionIndex: number | null
  factions: FaceitFactionPrefill[]
}

export interface AddResultPayload {
  opponent: string
  ourScore?: number
  opponentScore?: number
  mapName?: string
  notes?: string
  playedAtUtc: string
  category: MatchCategory
  tournamentId?: string | null
  leagueId?: string | null
  /** Present only when the coach analysed a demo first and picked a team — lets the server import a stat line for
   * every one of that team's players (connected to a roster account when their SteamID64 matches, unconnected otherwise). */
  demoRoundsPlayed?: number
  demoPlayers?: AnalyzedDemoPlayer[]
  ourTeamSteamIds?: string[]
  /** The analysis's parked timeline, moved to the new match on save. */
  pendingTimelineKey?: string
}

export interface PresignedDemoUpload {
  uploadUrl: string
  objectKey: string
}

/** Edits a logged result's metadata — never touches its imported player stat lines. */
export interface UpdateResultPayload {
  opponent: string
  ourScore: number
  opponentScore: number
  mapName?: string | null
  demoUrl?: string | null
  notes?: string | null
  playedAtUtc: string
  category: MatchCategory
  tournamentId?: string | null
  leagueId?: string | null
}

export const resultsApi = {
  getResults: () => apiClient.get<MatchResult[]>(API_ENDPOINTS.results),
  addResult: (payload: AddResultPayload) => apiClient.post<MatchResult>(API_ENDPOINTS.results, payload),
  updateResult: (matchResultId: string, payload: UpdateResultPayload) =>
    apiClient.patch<MatchResult>(API_ENDPOINTS.resultById(matchResultId), payload),
  deleteResult: (matchResultId: string) => apiClient.delete<void>(API_ENDPOINTS.resultById(matchResultId)),
  /** Starts a background analysis job (result: AnalyzeDemoResult); the multipart part keeps the original file name. */
  analyzeDemo: (demoFile: File) => {
    const formData = new FormData()
    formData.append('demo', demoFile)
    return apiClient.postForm<JobAccepted>(API_ENDPOINTS.analyzeResultDemo, formData)
  },
  /** For demos too large for analyzeDemo's own request — this asks the server for a time-limited URL, the caller
   * then PUTs the file straight to it (see uploadFileToPresignedUrl), bypassing this app's own server entirely. */
  presignDemoUpload: () => apiClient.post<PresignedDemoUpload>(API_ENDPOINTS.presignResultDemo, {}),
  /** Starts a background analysis job (result: AnalyzeDemoResult) of a presigned upload; `fileName` enables FACEIT recognition. */
  analyzeDemoFromStorage: (objectKey: string, fileName?: string) =>
    apiClient.post<JobAccepted>(API_ENDPOINTS.analyzeResultDemoFromStorage, { objectKey, fileName }),
}

/** Uploads a file (or any other blob, e.g. a pasted clipboard image) directly to a presigned object-storage URL —
 * deliberately not going through apiClient, since this request goes to a completely different origin (the storage
 * provider's own endpoint) and must carry neither our Bearer token nor a JSON Content-Type. */
export async function uploadFileToPresignedUrl(uploadUrl: string, file: Blob): Promise<void> {
  const response = await fetch(uploadUrl, { method: 'PUT', body: file })

  if (!response.ok) {
    throw new Error(`Wgrywanie pliku do magazynu nie powiodło się (status ${response.status}).`)
  }
}
