import { create } from 'zustand'
import { STORAGE_KEYS } from '../../../constants'
import { decodeSessionFromToken } from '../../../services/jwt'

interface AuthState {
  userId: string | null
  displayName: string | null
  role: string | null
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
  avatarUrl: restoredSession?.avatarUrl ?? null,
  inGameNickname: null,
  isAuthenticated: restoredSession !== null,
  loginWithToken: (token) => {
    const session = decodeSessionFromToken(token)

    if (!session) {
      return false
    }

    localStorage.setItem(STORAGE_KEYS.accessToken, token)
    set({
      userId: session.userId,
      displayName: session.displayName,
      role: session.role,
      avatarUrl: session.avatarUrl,
      inGameNickname: null,
      isAuthenticated: true,
    })
    return true
  },
  setInGameNickname: (nickname) => set({ inGameNickname: nickname }),
  clearSession: () => {
    localStorage.removeItem(STORAGE_KEYS.accessToken)
    set({
      userId: null,
      displayName: null,
      role: null,
      avatarUrl: null,
      inGameNickname: null,
      isAuthenticated: false,
    })
  },
}))
