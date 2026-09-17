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
    role: (userId: string) => `/api/roster/${userId}/role`,
    isCoach: (userId: string) => `/api/roster/${userId}/is-coach`,
    teamRole: (userId: string) => `/api/roster/${userId}/team-role`,
    rosterSlot: (userId: string) => `/api/roster/${userId}/roster-slot`,
    secondaryTeamRoles: (userId: string) => `/api/roster/${userId}/secondary-team-roles`,
    pinColor: (userId: string) => `/api/roster/${userId}/pin-color`,
    myNickname: '/api/roster/me/nickname',
    myPinColor: '/api/roster/me/pin-color',
    myPinMark: '/api/roster/me/pin-mark',
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
    assign: '/api/tasks',
    complete: (taskId: string) => `/api/tasks/${taskId}/complete`,
  },
  results: '/api/results',
  nades: '/api/nades',
  nadeById: (nadeId: string) => `/api/nades/${nadeId}`,
  mapStrategy: {
    positions: (mapName: string, side: string) => `/api/map-strategy/${mapName}/${side}`,
    set: '/api/map-strategy',
    remove: (positionId: string) => `/api/map-strategy/${positionId}`,
  },
  trainingMaterials: '/api/training-materials',
  matchStats: (matchResultId: string) => `/api/matches/${matchResultId}/stats`,
  statsMine: '/api/stats/mine',
  teamTrend: '/api/stats/team-trend',
  opponents: '/api/opponents',
} as const

export const STORAGE_KEYS = {
  accessToken: 'harnashub.accessToken',
} as const
