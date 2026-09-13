// The backend embeds claims under their full ClaimTypes URIs (no short-name outbound mapping applied).
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

export interface DecodedSession {
  userId: string
  displayName: string
  role: string
}

/** Decodes the session fields out of a JWT's payload without verifying its signature (verification is the server's job). */
export function decodeSessionFromToken(token: string): DecodedSession | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return {
      userId: payload.sub,
      displayName: payload[NAME_CLAIM],
      role: payload[ROLE_CLAIM],
    }
  } catch {
    return null
  }
}
