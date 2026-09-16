import { useEffect } from 'react'
import { useIsGuest } from '../../auth/hooks/useIsGuest'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useRoster } from './useRoster'

/**
 * Mirrors the signed-in member's in-game nickname into the session store — the JWT doesn't carry it,
 * so it's read from the (already cached and deduped) roster query instead of a dedicated request.
 */
export function useSyncOwnNickname() {
  const userId = useAuthStore((state) => state.userId)
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)
  const setInGameNickname = useAuthStore((state) => state.setInGameNickname)
  const isGuest = useIsGuest()
  const { data: roster } = useRoster(isAuthenticated && !isGuest)

  useEffect(() => {
    if (!roster || !userId) {
      return
    }

    const me = roster.find((member) => member.id === userId)

    if (me) {
      setInGameNickname(me.inGameNickname)
    }
  }, [roster, userId, setInGameNickname])
}
