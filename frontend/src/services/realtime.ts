import type { QueryClient } from '@tanstack/react-query'
import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { API_SETTINGS, STORAGE_KEYS } from '../constants'

/** Maps a server-pushed topic to the TanStack Query key(s) it should invalidate. */
function invalidateForTopic(queryClient: QueryClient, topic: string) {
  if (topic.startsWith('availability:')) {
    const eventId = topic.split(':')[1]
    queryClient.invalidateQueries({ queryKey: ['calendar', 'events', eventId, 'availability'] })
    return
  }

  if (topic.startsWith('match-analysis:')) {
    const matchResultId = topic.split(':')[1]
    queryClient.invalidateQueries({ queryKey: ['match-analysis', matchResultId] })
    return
  }

  // Background job progress: "job:{id}" refetches that job; "jobs:{userId}" (everything a user started) needs no cache today.
  if (topic.startsWith('job:')) {
    queryClient.invalidateQueries({ queryKey: ['jobs', topic.slice('job:'.length)] })
    return
  }

  if (topic.startsWith('jobs:')) {
    return
  }

  if (topic.startsWith('match-stats:')) {
    const matchResultId = topic.split(':')[1]
    queryClient.invalidateQueries({ queryKey: ['stats', 'match', matchResultId] })
    return
  }

  const topLevelKeys: Record<string, string[]> = {
    calendar: ['calendar'],
    // No third key element on purpose: this refreshes every cached week, not just the visible one.
    'availability-week': ['availability', 'week'],
    tasks: ['tasks'],
    dashboard: ['dashboard'],
    results: ['results'],
    stats: ['stats'],
    roster: ['roster'],
    nades: ['nades'],
    'map-strategy': ['map-strategy'],
    tactics: ['tactics'],
    materials: ['materials'],
    opponents: ['opponents'],
    'map-pool': ['map-pool'],
    veto: ['veto'],
    'game-plan': ['game-plan'],
    'analysis-boards': ['analysis-boards'],
    attendance: ['attendance'],
  }

  const queryKey = topLevelKeys[topic]

  if (queryKey) {
    queryClient.invalidateQueries({ queryKey })
  }

  // Opponent profiles aggregate results and scheduled events, so those topics must refresh them too.
  if (topic === 'results' || topic === 'calendar') {
    queryClient.invalidateQueries({ queryKey: ['opponents'] })
  }

  // The map pool's per-map record is computed from logged results; tactic counts come from the tactics library.
  if (topic === 'results' || topic === 'tactics') {
    queryClient.invalidateQueries({ queryKey: ['map-pool'] })
  }

  // Veto suggestions are scored from the pool status, results and tactic counts.
  if (topic === 'results' || topic === 'tactics' || topic === 'map-pool') {
    queryClient.invalidateQueries({ queryKey: ['veto'] })
  }

  // Game plans show tactic and board names, so a rename or delete there must refresh them.
  if (topic === 'tactics' || topic === 'analysis-boards') {
    queryClient.invalidateQueries({ queryKey: ['game-plan'] })
  }
}

/** Opens a SignalR connection to the team hub and wires incoming "update" topics to cache invalidation. */
export function connectRealtime(queryClient: QueryClient): HubConnection {
  const token = localStorage.getItem(STORAGE_KEYS.accessToken)

  const connection = new HubConnectionBuilder()
    .withUrl(`${API_SETTINGS.baseUrl}/hubs/team`, { accessTokenFactory: () => token ?? '' })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  connection.on('update', (topic: string) => invalidateForTopic(queryClient, topic))
  connection.onreconnecting(() => (realtimeConnected = false))
  connection.onreconnected(() => (realtimeConnected = true))
  connection.onclose(() => (realtimeConnected = false))

  connection
    .start()
    .then(() => (realtimeConnected = true))
    .catch((error) => console.error('Nie udało się połączyć z live-update:', error))

  return connection
}

let realtimeConnected = false

/** Whether the live-update connection is up — background job polling slows down while pushes arrive. */
export function isRealtimeConnected(): boolean {
  return realtimeConnected
}
