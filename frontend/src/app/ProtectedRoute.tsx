import type { PropsWithChildren } from 'react'
import { Navigate } from 'react-router-dom'
import { useIsGuest } from '../features/auth/hooks/useIsGuest'
import { useAuthStore } from '../features/auth/stores/useAuthStore'
import { PendingAccessPage } from './PendingAccessPage'

interface ProtectedRouteProps extends PropsWithChildren {
  /** Guests only ever reach the "pending access" screen; set to false for a page that a Guest may open. */
  requiresTeamAccess?: boolean
}

/** Redirects to /login without a session, and holds Guests on the "pending access" screen. */
export function ProtectedRoute({ children, requiresTeamAccess = true }: ProtectedRouteProps) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  const isGuest = useIsGuest()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  if (requiresTeamAccess && isGuest) {
    return <PendingAccessPage />
  }

  return children
}
