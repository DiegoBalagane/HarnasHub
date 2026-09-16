import { useEffect } from 'react'
import { useAuthStore } from '../features/auth/stores/useAuthStore'
import { authApi } from '../services/authApi'

/**
 * Re-fetches the caller's role-bearing JWT on app start and whenever the tab regains focus, so a
 * role change made by a Manager (e.g. Guest -> Player) applies without the user logging out/in again.
 * Runs for every authenticated user, Guests included — unlike RealtimeSync, which they can't reach.
 * Renders nothing.
 */
export function SessionRefresh() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  const loginWithToken = useAuthStore((state) => state.loginWithToken)

  useEffect(() => {
    if (!isAuthenticated) return

    async function refresh() {
      try {
        const session = await authApi.refresh()
        loginWithToken(session.accessToken)
      } catch {
        // A 401 already triggers a full logout+redirect inside apiClient; any other failure just
        // leaves the current (still valid) token in place until the next refresh attempt.
      }
    }

    refresh()

    function handleFocus() {
      refresh()
    }

    window.addEventListener('focus', handleFocus)
    document.addEventListener('visibilitychange', handleFocus)

    return () => {
      window.removeEventListener('focus', handleFocus)
      document.removeEventListener('visibilitychange', handleFocus)
    }
  }, [isAuthenticated, loginWithToken])

  return null
}
