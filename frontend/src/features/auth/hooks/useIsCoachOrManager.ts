import { useAuthStore } from '../stores/useAuthStore'

/** True for a Manager (by access level) or anyone tagged as the team's Coach, independent of access level. */
export function useIsCoachOrManager(): boolean {
  return useAuthStore((state) => state.role === 'Manager' || state.isCoach)
}
