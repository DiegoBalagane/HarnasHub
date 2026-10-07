// Files at or under this go straight through /api/results/analyze-demo (proven, simple); bigger ones go through
// the presigned-upload-to-object-storage path instead, since the hosting platform's own edge proxy rejects large
// request bodies well before this app's own (much higher) Kestrel limit ever comes into play.
export const DEMO_DIRECT_UPLOAD_MAX_BYTES = 250 * 1024 * 1024

/** Server-side cap on an event game plan's notes (SetEventGamePlanCommandValidator). */
export const GAME_PLAN_NOTES_MAX_LENGTH = 4000

/** Server-side cap on the pasted FACEIT link source (LinkOpponentFaceitCommandValidator). */
export const FACEIT_LINK_SOURCE_MAX_LENGTH = 1000

/** Opponent demos: default and server-side max (DownloadOpponentDemosCommand.MaxCount) demos per FACEIT download run. */
export const OPPONENT_DEMO_DOWNLOAD = { defaultCount: 3, maxCount: 5 } as const

/** FACEIT match room page — where a demo can be downloaded by hand while the Downloads API isn't available. */
export const faceitMatchRoomUrl = (faceitMatchId: string) => `https://www.faceit.com/en/cs2/room/${encodeURIComponent(faceitMatchId)}`

/** 2D round replay: playback speeds, seconds skipped by ←/→ (Shift = fast) and dot/label sizes in CSS pixels. */
export const REPLAY_SETTINGS = { speeds: [1, 2, 4], seekStepSeconds: 1, fastSeekStepSeconds: 5, dotRadiusPx: 6, labelFontPx: 11 } as const

export const API_SETTINGS = {
  // Empty string = same-origin relative requests. That's the production default: the backend
  // serves this built frontend itself (see docs/DEPLOYMENT.md), so there's no separate API host.
  // Local dev overrides this via .env.development (VITE_API_BASE_URL) to point at the dotnet process.
  baseUrl: import.meta.env.VITE_API_BASE_URL ?? '',
  timeoutMs: 10_000,
} as const

export const API_ENDPOINTS = {
  auth: {
    discordLogin: '/api/auth/discord/login',
    refresh: '/api/auth/refresh',
  },
  roster: {
    list: '/api/roster',
    byId: (userId: string) => `/api/roster/${userId}`,
    role: (userId: string) => `/api/roster/${userId}/role`,
    isCoach: (userId: string) => `/api/roster/${userId}/is-coach`,
    teamRole: (userId: string) => `/api/roster/${userId}/team-role`,
    rosterSlot: (userId: string) => `/api/roster/${userId}/roster-slot`,
    secondaryTeamRoles: (userId: string) => `/api/roster/${userId}/secondary-team-roles`,
    pinColor: (userId: string) => `/api/roster/${userId}/pin-color`,
    steamId64: (userId: string) => `/api/roster/${userId}/steam-id`,
    visibility: (userId: string) => `/api/roster/${userId}/visibility`,
    faceitNickname: (userId: string) => `/api/roster/${userId}/faceit-nickname`,
    myNickname: '/api/roster/me/nickname',
    myPinColor: '/api/roster/me/pin-color',
    myPinMark: '/api/roster/me/pin-mark',
    mySteamId64: '/api/roster/me/steam-id',
  },
  admin: {
    status: '/api/admin/status',
  },
  dashboard: '/api/dashboard',
  calendar: {
    events: '/api/calendar/events',
    event: (eventId: string) => `/api/calendar/events/${eventId}`,
    availability: (eventId: string) => `/api/calendar/events/${eventId}/availability`,
  },
  availability: {
    week: '/api/availability/week',
    day: '/api/availability/day',
    vacations: '/api/availability/vacations',
    vacationById: (vacationId: string) => `/api/availability/vacations/${vacationId}`,
  },
  tasks: {
    mine: '/api/tasks/mine',
    all: '/api/tasks/all',
    assign: '/api/tasks',
    byId: (taskId: string) => `/api/tasks/${taskId}`,
    submit: (taskId: string) => `/api/tasks/${taskId}/submit`,
    approve: (taskId: string) => `/api/tasks/${taskId}/approve`,
    reject: (taskId: string) => `/api/tasks/${taskId}/reject`,
  },
  results: '/api/results',
  resultById: (matchResultId: string) => `/api/results/${matchResultId}`,
  matchTimeline: (matchResultId: string) => `/api/results/${matchResultId}/timeline`,
  matchInsights: (matchResultId: string) => `/api/results/${matchResultId}/insights`,
  matchAnalysis: (matchResultId: string) => `/api/results/${matchResultId}/analysis`,
  excludeAnalysisPlayer: (matchResultId: string) => `/api/results/${matchResultId}/analysis/exclude`,
  includeAnalysisPlayer: (matchResultId: string) => `/api/results/${matchResultId}/analysis/include`,
  mapAnalytics: (mapName: string) => `/api/maps/${mapName}/analytics`,
  roundReplay: (matchResultId: string, roundNumber: number) =>
    `/api/results/${matchResultId}/rounds/${roundNumber}/replay`,
  matchTacticMatches: (matchResultId: string) => `/api/results/${matchResultId}/tactic-matches`,
  attachResultDemo: (matchResultId: string) => `/api/results/${matchResultId}/demo`,
  analyzeResultDemo: '/api/results/analyze-demo',
  presignResultDemo: '/api/results/analyze-demo/presign',
  analyzeResultDemoFromStorage: '/api/results/analyze-demo/from-storage',
  tournaments: '/api/tournaments',
  tournamentById: (tournamentId: string) => `/api/tournaments/${tournamentId}`,
  leagues: '/api/leagues',
  leagueById: (leagueId: string) => `/api/leagues/${leagueId}`,
  nades: '/api/nades',
  nadeById: (nadeId: string) => `/api/nades/${nadeId}`,
  nadePosition: (nadeId: string) => `/api/nades/${nadeId}/position`,
  mapStrategy: {
    positions: (mapName: string, side: string) => `/api/map-strategy/${mapName}/${side}`,
    set: '/api/map-strategy',
    remove: (positionId: string) => `/api/map-strategy/${positionId}`,
    textAnnotations: (mapName: string, side: string) => `/api/map-strategy/${mapName}/${side}/text-annotations`,
    addTextAnnotation: '/api/map-strategy/text-annotations',
    textAnnotationById: (annotationId: string) => `/api/map-strategy/text-annotations/${annotationId}`,
  },
  tactics: {
    list: '/api/tactics',
    byId: (tacticId: string) => `/api/tactics/${tacticId}`,
    importDemoExtract: '/api/tactics/import-demo/extract',
    importDemo: '/api/tactics/import-demo',
    effectiveness: (mapName: string) => `/api/tactics/effectiveness?mapName=${mapName}`,
  },
  analysisBoards: {
    list: (mapName?: string) => (mapName ? `/api/analysis-boards?mapName=${mapName}` : '/api/analysis-boards'),
    byId: (boardId: string) => `/api/analysis-boards/${boardId}`,
    presignImageUpload: '/api/analysis-boards/presign-image-upload',
    fromRound: '/api/analysis-boards/from-round',
  },
  trainingMaterials: '/api/training-materials',
  matchStats: (matchResultId: string) => `/api/matches/${matchResultId}/stats`,
  statsMine: '/api/stats/mine',
  teamTrend: '/api/stats/team-trend',
  statsLeaderboard: (category?: string) =>
    category ? `/api/stats/leaderboard?category=${category}` : '/api/stats/leaderboard',
  statsAdvanced: (category?: string, map?: string, last?: number) => {
    const params = new URLSearchParams()
    if (category) params.set('category', category)
    if (map) params.set('map', map)
    if (last) params.set('last', String(last))
    const query = params.toString()
    return query ? `/api/stats/advanced?${query}` : '/api/stats/advanced'
  },
  gamePlan: (eventId: string) => `/api/game-plans/${eventId}`,
  veto: {
    suggestion: (opponent: string) => `/api/veto/suggestion?opponent=${encodeURIComponent(opponent)}`,
    event: (eventId: string) => `/api/veto/events/${eventId}`,
  },
  mapPool: {
    list: (category?: string) => (category ? `/api/map-pool?category=${category}` : '/api/map-pool'),
    byMap: (mapName: string) => `/api/map-pool/${mapName}`,
  },
  opponents: {
    list: '/api/opponents',
    listWithHidden: '/api/opponents?includeHidden=true',
    deletePreview: (name: string) => `/api/opponents/delete-preview?name=${encodeURIComponent(name)}`,
    remove: (name: string, includeHistory: boolean) =>
      `/api/opponents?name=${encodeURIComponent(name)}&includeHistory=${includeHistory}`,
    hide: '/api/opponents/hide',
    unhide: '/api/opponents/unhide',
    rename: '/api/opponents/rename',
    profile: (name: string) => `/api/opponents/profile?name=${encodeURIComponent(name)}`,
    notes: '/api/opponents/notes',
    noteById: (noteId: string) => `/api/opponents/notes/${noteId}`,
    report: (name: string) => `/api/opponents/report?name=${encodeURIComponent(name)}`,
    reportLink: '/api/opponents/report/link',
    reportRefresh: (name: string) => `/api/opponents/report/refresh?name=${encodeURIComponent(name)}`,
    demos: (name: string) => `/api/opponents/report/demos?name=${encodeURIComponent(name)}`,
    analyzeDemo: '/api/opponents/report/demos',
    demoById: (demoId: string) => `/api/opponents/report/demos/${demoId}`,
    demoTeam: (demoId: string) => `/api/opponents/report/demos/${demoId}/team`,
    demoReplay: (demoId: string, roundNumber: number) =>
      `/api/opponents/report/demos/${demoId}/rounds/${roundNumber}/replay`,
    faceitDemoDownload: '/api/opponents/report/demos/faceit-download',
  },
  teamInfo: {
    list: '/api/team-info',
    byId: (id: string) => `/api/team-info/${id}`,
    order: '/api/team-info/order',
  },
  attendance: {
    summary: '/api/attendance/summary',
    incidents: (userId?: string) =>
      userId ? `/api/attendance/incidents?userId=${userId}` : '/api/attendance/incidents',
    incidentById: (incidentId: string) => `/api/attendance/incidents/${incidentId}`,
  },
  jobs: {
    byId: (jobId: string) => `/api/jobs/${jobId}`,
  },
} as const

/** Background jobs: status poll interval without a live SignalR connection, and the slower safety poll while connected
 * (pushes on the `job:{id}` topic normally drive the refetches). */
/** When a new deployed build is checked for and how soon after opening it may be applied without asking. */
export const PWA_UPDATE_SETTINGS = { checkIntervalMs: 60_000, applyImmediatelyWithinMs: 10_000 } as const

export const JOB_SETTINGS = { pollIntervalMs: 2_000, connectedPollIntervalMs: 10_000 } as const

export const STORAGE_KEYS = {
  accessToken: 'harnashub.accessToken',
} as const


/** Sidebar navigation: localStorage key of the collapsed flag and the two widths (px) the layout offsets content by. */
export const SIDEBAR_SETTINGS = {
  storageKey: 'harnashub.sidebarCollapsed',
  expandedWidthPx: 240,
  collapsedWidthPx: 64,
} as const
