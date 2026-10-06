import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'
import type { JobAccepted } from './jobsApi'
import type { MapName } from './nadesApi'
import type { ConfidenceLevel } from './opponentReportApi'
import { resultsApi } from './resultsApi'

/** "A" started the demo on T, "B" on CT. */
export type DemoTeam = 'A' | 'B'
export type MapArea = 'A' | 'B' | 'Mid'
export type DemoGrenadeType = 'Smoke' | 'Flash' | 'HighExplosive' | 'Molotov' | 'Incendiary' | 'Decoy'

/** One team of a demo as of round 1. */
export interface DemoTeamRoster {
  team: DemoTeam
  steamIds: string[]
  names: string[]
}

/** One analysed opponent demo; when `teamResolved` is false the coach picks the opponent's team from `teams`. */
export interface OpponentDemo {
  id: string
  mapName: MapName | null
  rawMapName: string | null
  playedAtUtc: string | null
  source: 'Upload' | 'FaceitDownload'
  faceitMatchId: string | null
  roundsCount: number
  teamResolved: boolean
  opponentTeam: DemoTeam | null
  teams: DemoTeamRoster[]
  createdAtUtc: string
}

/** The opponent's demo list plus what the section may offer. */
export interface OpponentDemos {
  storageConfigured: boolean
  autoDownloadAvailable: boolean
  demos: OpponentDemo[]
}

export interface OpponentDemoDownloadResult {
  analysed: number
  failed: number
  candidates: number
  demos: OpponentDemo[]
}

/** One bar of a distribution. */
export interface TendencyShare {
  label: string
  count: number
  percent: number
}

/** A radar point (fractions in [0,1]). */
export interface TendencyPoint {
  x: number
  y: number
  area: MapArea | null
  awp: boolean
}

export interface EntryArrow {
  area: MapArea
  x: number
  y: number
  count: number
  percent: number
}

export interface GrenadeCluster {
  type: DemoGrenadeType
  x: number
  y: number
  throws: number
  rounds: number
  perRoundPercent: number
  area: MapArea | null
}

export interface PistolTendency {
  pistolRounds: number
  pistolWins: number
  tPistolTargets: TendencyShare[]
  lostPistols: number
  afterLostPistolBuys: TendencyShare[]
}

export interface TSideTendencies {
  rounds: number
  confidence: ConfidenceLevel
  targets: TendencyShare[]
  /** Labels "Fast" (<35 s), "Mid", "Late" (>75 s). */
  execTiming: TendencyShare[]
  averageExecSecond: number | null
  entries: EntryArrow[]
  grenadeClusters: GrenadeCluster[]
  pistol: PistolTendency
}

export interface CtSideTendencies {
  rounds: number
  confidence: ConfidenceLevel
  setups: TendencyShare[]
  stacks: TendencyShare[]
  setupPositions: TendencyPoint[]
  awpAreas: TendencyShare[]
  awpKillPositions: TendencyPoint[]
  earlyKillRounds: number
  earlyKillPercent: number
  postPlantRounds: number
  retakes: number
  saves: number
  retakesWon: number
}

export interface PlayerTendency {
  steamId64: string
  name: string
  rounds: number
  kills: number
  entryRate: number
  openingWinRate: number | null
  awpKills: number
  awpKillShare: number
  clutchAttempts: number
  clutchWins: number
  role: 'AWP' | 'Entry' | 'Clutch' | null
}

export interface AntiStratSuggestion {
  kind: string
  side: 'T' | 'CT' | 'Players'
  text: string
  evidence: string
  confidence: ConfidenceLevel
}

/** Everything learnt from the opponent's demos on one map. */
export interface MapTendencies {
  mapName: MapName
  demos: number
  rounds: number
  hasZones: boolean
  tSpawnX: number | null
  tSpawnY: number | null
  t: TSideTendencies
  ct: CtSideTendencies
  players: PlayerTendency[]
  suggestions: AntiStratSuggestion[]
}

/** PUTs a file to a presigned URL reporting upload progress (0–1) — fetch can't report upload progress, XHR can. */
export function uploadWithProgress(
  uploadUrl: string,
  file: Blob,
  onProgress: (fraction: number) => void,
): Promise<void> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest()
    xhr.open('PUT', uploadUrl)
    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable) onProgress(event.loaded / event.total)
    }
    xhr.onload = () =>
      xhr.status >= 200 && xhr.status < 300
        ? resolve()
        : reject(new Error(`Wgrywanie pliku do magazynu nie powiodło się (status ${xhr.status}).`))
    xhr.onerror = () => reject(new Error('Wgrywanie pliku do magazynu nie powiodło się.'))
    xhr.send(file)
  })
}

export const opponentDemosApi = {
  getDemos: (name: string) => apiClient.get<OpponentDemos>(API_ENDPOINTS.opponents.demos(name)),
  /** Uploads one .dem straight to object storage (with progress), then starts its background analysis (result: OpponentDemo);
   * the original file name lets the server recognise a FACEIT match and confirm the opponent side. */
  uploadAndAnalyze: async (opponentName: string, file: File, onProgress: (fraction: number) => void) => {
    const { uploadUrl, objectKey } = await resultsApi.presignDemoUpload()
    await uploadWithProgress(uploadUrl, file, onProgress)
    return apiClient.post<JobAccepted>(API_ENDPOINTS.opponents.analyzeDemo, { opponentName, objectKey, fileName: file.name })
  },
  setTeam: (demoId: string, team: DemoTeam) =>
    apiClient.put<OpponentDemo>(API_ENDPOINTS.opponents.demoTeam(demoId), { team }),
  deleteDemo: (demoId: string) => apiClient.delete<void>(API_ENDPOINTS.opponents.demoById(demoId)),
  /** Starts a background download + analysis job (result: OpponentDemoDownloadResult). */
  faceitDownload: (opponentName: string, maps: MapName[], count: number) =>
    apiClient.post<JobAccepted>(API_ENDPOINTS.opponents.faceitDemoDownload, {
      opponentName,
      maps,
      count,
    }),
}
