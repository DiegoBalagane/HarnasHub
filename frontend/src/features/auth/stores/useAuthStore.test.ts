import { beforeEach, describe, expect, it } from 'vitest'
import { STORAGE_KEYS } from '../../../constants'
import { useAuthStore } from './useAuthStore'

const ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
const NAME = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'

function token(sub: string): string {
  return `h.${btoa(JSON.stringify({ sub, [NAME]: 'Ann', [ROLE]: ['Player', 'Coach'] }))}.s`
}

describe('useAuthStore', () => {
  beforeEach(() => useAuthStore.getState().clearSession())

  it('rejects an invalid token without touching the session', () => {
    expect(useAuthStore.getState().loginWithToken('nope')).toBe(false)
    expect(useAuthStore.getState().isAuthenticated).toBe(false)
  })

  it('stores the token and decodes the session on login', () => {
    expect(useAuthStore.getState().loginWithToken(token('u1'))).toBe(true)
    const state = useAuthStore.getState()
    expect(state).toMatchObject({ userId: 'u1', displayName: 'Ann', role: 'Player', isCoach: true, isAuthenticated: true })
    expect(localStorage.getItem(STORAGE_KEYS.accessToken)).toBeTruthy()
  })

  it('keeps the nickname on a refresh of the same account but resets it for another one', () => {
    useAuthStore.getState().loginWithToken(token('u1'))
    useAuthStore.getState().setInGameNickname('ann')
    useAuthStore.getState().loginWithToken(token('u1'))
    expect(useAuthStore.getState().inGameNickname).toBe('ann')
    useAuthStore.getState().loginWithToken(token('u2'))
    expect(useAuthStore.getState().inGameNickname).toBeNull()
  })

  it('clears the whole session and the stored token on logout', () => {
    useAuthStore.getState().loginWithToken(token('u1'))
    useAuthStore.getState().clearSession()
    expect(useAuthStore.getState().isAuthenticated).toBe(false)
    expect(localStorage.getItem(STORAGE_KEYS.accessToken)).toBeNull()
  })
})
