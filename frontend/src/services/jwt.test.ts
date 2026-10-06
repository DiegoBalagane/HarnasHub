import { describe, expect, it } from 'vitest'
import { decodeSessionFromToken } from './jwt'

const ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
const NAME = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'

function tokenWith(payload: object): string {
  return `h.${btoa(JSON.stringify(payload))}.s`
}

describe('decodeSessionFromToken', () => {
  it('decodes a single role claim', () => {
    expect(decodeSessionFromToken(tokenWith({ sub: 'u1', [NAME]: 'Ann', [ROLE]: 'Player' }))).toEqual({
      userId: 'u1',
      displayName: 'Ann',
      role: 'Player',
      isCoach: false,
      avatarUrl: null,
    })
  })

  it('splits the Coach role out of a role array', () => {
    const session = decodeSessionFromToken(
      tokenWith({ sub: 'u1', [NAME]: 'Ann', [ROLE]: ['Coach', 'Manager'], avatar_url: 'http://a/b.png' }),
    )
    expect(session).toMatchObject({ role: 'Manager', isCoach: true, avatarUrl: 'http://a/b.png' })
  })

  it('returns null for a malformed token', () => {
    expect(decodeSessionFromToken('garbage')).toBeNull()
  })
})
