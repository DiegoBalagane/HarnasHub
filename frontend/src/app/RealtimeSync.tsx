import { useEffect } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '../features/auth/stores/useAuthStore'
import { connectRealtime } from '../services/realtime'

/** Keeps a live SignalR connection open while authenticated, so team data updates without polling. Renders nothing. */
export function RealtimeSync() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!isAuthenticated) return

    const connection = connectRealtime(queryClient)

    return () => {
      connection.stop()
    }
  }, [isAuthenticated, queryClient])

  return null
}
