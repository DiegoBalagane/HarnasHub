import { create } from 'zustand'
import { STORAGE_KEYS } from '../../../constants'
import type { AuthResult } from '../../../services/authApi'
import { decodeSessionFromToken } from '../../../services/jwt'

interface AuthState {
  userId: string | null
  displayName: string | null
  role: string | null
  isAuthenticated: boolean
  setSession: (result: AuthResult) => void
  clearSession: () => void
}

const existingToken = localStorage.getItem(STORAGE_KEYS.accessToken)
const restoredSession = existingToken ? decodeSessionFromToken(existingToken) : null

/** Holds the current session. The JWT is the source of truth in localStorage; userId/displayName/role are decoded from it on load so a page reload doesn't drop them. */
export const useAuthStore = create<AuthState>((set) => ({
  userId: restoredSession?.userId ?? null,
  displayName: restoredSession?.displayName ?? null,
  role: restoredSession?.role ?? null,
  isAuthenticated: restoredSession !== null,
  setSession: (result) => {
    localStorage.setItem(STORAGE_KEYS.accessToken, result.accessToken)
    set({
      userId: result.userId,
      displayName: result.displayName,
      role: result.role,
      isAuthenticated: true,
    })
  },
  clearSession: () => {
    localStorage.removeItem(STORAGE_KEYS.accessToken)
    set({ userId: null, displayName: null, role: null, isAuthenticated: false })
  },
}))
