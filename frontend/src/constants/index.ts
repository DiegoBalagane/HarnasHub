// Files at or under this go straight through /api/results/analyze-demo (proven, simple); bigger ones go through
// the presigned-upload-to-object-storage path instead, since the hosting platform's own edge proxy rejects large
// request bodies well before this app's own (much higher) Kestrel limit ever comes into play.
export const DEMO_DIRECT_UPLOAD_MAX_BYTES = 250 * 1024 * 1024

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
    myNickname: '/api/roster/me/nickname',
    myPinColor: '/api/roster/me/pin-color',
    myPinMark: '/api/roster/me/pin-mark',
    mySteamId64: '/api/roster/me/steam-id',
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
  },
  analysisBoards: {
    list: (mapName?: string) => (mapName ? `/api/analysis-boards?mapName=${mapName}` : '/api/analysis-boards'),
    byId: (boardId: string) => `/api/analysis-boards/${boardId}`,
    presignImageUpload: '/api/analysis-boards/presign-image-upload',
  },
  trainingMaterials: '/api/training-materials',
  matchStats: (matchResultId: string) => `/api/matches/${matchResultId}/stats`,
  statsMine: '/api/stats/mine',
  teamTrend: '/api/stats/team-trend',
  statsLeaderboard: (category?: string) =>
    category ? `/api/stats/leaderboard?category=${category}` : '/api/stats/leaderboard',
  opponents: '/api/opponents',
  attendance: {
    summary: '/api/attendance/summary',
    incidents: (userId?: string) =>
      userId ? `/api/attendance/incidents?userId=${userId}` : '/api/attendance/incidents',
    incidentById: (incidentId: string) => `/api/attendance/incidents/${incidentId}`,
  },
} as const

export const STORAGE_KEYS = {
  accessToken: 'harnashub.accessToken',
} as const
