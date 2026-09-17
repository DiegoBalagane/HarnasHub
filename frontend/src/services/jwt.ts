// The backend embeds claims under their full ClaimTypes URIs (no short-name outbound mapping applied),
// except avatar_url which was never a ClaimTypes constant to begin with.
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
const AVATAR_CLAIM = 'avatar_url'

export interface DecodedSession {
  userId: string
  displayName: string
  /** Access level only (Guest/Player/Manager) — the "Coach" claim, if present, is split out into `isCoach`. */
  role: string
  /** True when the backend also issued a "Coach" role claim alongside the access-level one. */
  isCoach: boolean
  avatarUrl: string | null
}

// .NET serializes multiple claims sharing a type as a JSON array under that claim's key, so a user who is
// both an access level (Manager/Player/Guest) and Coach ends up with an array here instead of a single string.
function claimAsList(value: unknown): string[] {
  if (Array.isArray(value)) return value
  if (typeof value === 'string') return [value]
  return []
}

/** Decodes the session fields out of a JWT's payload without verifying its signature (verification is the server's job). */
export function decodeSessionFromToken(token: string): DecodedSession | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    const roles = claimAsList(payload[ROLE_CLAIM])

    return {
      userId: payload.sub,
      displayName: payload[NAME_CLAIM],
      role: roles.find((claimRole) => claimRole !== 'Coach') ?? '',
      isCoach: roles.includes('Coach'),
      avatarUrl: payload[AVATAR_CLAIM] ?? null,
    }
  } catch {
    return null
  }
}
