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
    materials: ['materials'],
    opponents: ['opponent-notes'],
    'analysis-boards': ['analysis-boards'],
  }

  const queryKey = topLevelKeys[topic]

  if (queryKey) {
    queryClient.invalidateQueries({ queryKey })
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

  connection.start().catch((error) => console.error('Nie udało się połączyć z live-update:', error))

  return connection
}
