import { useAuthStore } from '../stores/useAuthStore'

/** Role every account starts with after its first Discord sign-in, before a Manager grants real access. */
export const GUEST_ROLE = 'Guest'

/** True while the signed-in account is still a Guest and must not see any team data. */
export function useIsGuest(): boolean {
  return useAuthStore((state) => state.role === GUEST_ROLE)
}
