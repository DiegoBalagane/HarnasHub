export const API_SETTINGS = {
  baseUrl: import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:5001',
  timeoutMs: 10_000,
} as const

export const API_ENDPOINTS = {
  auth: {
    register: '/api/auth/register',
    login: '/api/auth/login',
  },
  roster: '/api/roster',
  rosterRole: (userId: string) => `/api/roster/${userId}/role`,
  dashboard: '/api/dashboard',
  calendar: {
    events: '/api/calendar/events',
    availability: (eventId: string) => `/api/calendar/events/${eventId}/availability`,
  },
  tasks: {
    mine: '/api/tasks/mine',
    assign: '/api/tasks',
    complete: (taskId: string) => `/api/tasks/${taskId}/complete`,
  },
  results: '/api/results',
  nades: '/api/nades',
  nadeById: (nadeId: string) => `/api/nades/${nadeId}`,
  trainingMaterials: '/api/training-materials',
  matchStats: (matchResultId: string) => `/api/matches/${matchResultId}/stats`,
  statsMine: '/api/stats/mine',
  teamTrend: '/api/stats/team-trend',
  opponents: '/api/opponents',
} as const

export const STORAGE_KEYS = {
  accessToken: 'harnashub.accessToken',
} as const
