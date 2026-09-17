import { create } from 'zustand'
import { STORAGE_KEYS } from '../../../constants'
import { decodeSessionFromToken } from '../../../services/jwt'

interface AuthState {
  userId: string | null
  displayName: string | null
  /** Access level only (Guest/Player/Manager) — see `isCoach` for the independent team-function flag. */
  role: string | null
  /** Whether this account is tagged as the team's Coach, independent of its access level. */
  isCoach: boolean
  avatarUrl: string | null
  /** Not part of the JWT — loaded from the roster after sign-in, see useSyncOwnNickname. */
  inGameNickname: string | null
  isAuthenticated: boolean
  loginWithToken: (token: string) => boolean
  setInGameNickname: (nickname: string | null) => void
  clearSession: () => void
}

const existingToken = localStorage.getItem(STORAGE_KEYS.accessToken)
const restoredSession = existingToken ? decodeSessionFromToken(existingToken) : null

/** Holds the current session. The JWT (from Discord OAuth) is the source of truth in localStorage; every other field is decoded from it. */
export const useAuthStore = create<AuthState>((set) => ({
  userId: restoredSession?.userId ?? null,
  displayName: restoredSession?.displayName ?? null,
  role: restoredSession?.role ?? null,
  isCoach: restoredSession?.isCoach ?? false,
  avatarUrl: restoredSession?.avatarUrl ?? null,
  inGameNickname: null,
  isAuthenticated: restoredSession !== null,
  loginWithToken: (token) => {
    const session = decodeSessionFromToken(token)

    if (!session) {
      return false
    }

    localStorage.setItem(STORAGE_KEYS.accessToken, token)
    set((state) => ({
      userId: session.userId,
      displayName: session.displayName,
      role: session.role,
      isCoach: session.isCoach,
      avatarUrl: session.avatarUrl,
      // A token refresh (SessionRefresh runs this on every focus/visibility change) re-decodes the
      // same session — resetting this would wipe it before useSyncOwnNickname gets a chance to
      // re-populate it. Only a genuine sign-in as a different account should clear it.
      inGameNickname: state.userId === session.userId ? state.inGameNickname : null,
      isAuthenticated: true,
    }))
    return true
  },
  setInGameNickname: (nickname) => set({ inGameNickname: nickname }),
  clearSession: () => {
    localStorage.removeItem(STORAGE_KEYS.accessToken)
    set({
      userId: null,
      displayName: null,
      role: null,
      isCoach: false,
      avatarUrl: null,
      inGameNickname: null,
      isAuthenticated: false,
    })
  },
}))
