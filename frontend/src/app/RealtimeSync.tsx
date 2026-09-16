import { useEffect } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { useIsGuest } from '../features/auth/hooks/useIsGuest'
import { useAuthStore } from '../features/auth/stores/useAuthStore'
import { connectRealtime } from '../services/realtime'

/** Keeps a live SignalR connection open for team members, so their data updates without polling. Renders nothing. */
export function RealtimeSync() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  const isGuest = useIsGuest()
  const queryClient = useQueryClient()

  useEffect(() => {
    // Guests are rejected by the hub's TeamMember policy, so don't even attempt (and retry) a connection.
    if (!isAuthenticated || isGuest) return

    const connection = connectRealtime(queryClient)

    return () => {
      connection.stop()
    }
  }, [isAuthenticated, isGuest, queryClient])

  return null
}
