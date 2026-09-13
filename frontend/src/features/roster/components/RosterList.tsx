import { useAuthStore } from '../../auth/stores/useAuthStore'
import type { TeamRole } from '../../../services/rosterApi'
import { useRoster, useUpdateRole } from '../hooks/useRoster'

const roleLabels: Record<TeamRole, string> = {
  Player: 'Zawodnik',
  Coach: 'Coach',
  Manager: 'Manager',
}

const roles: TeamRole[] = ['Player', 'Coach', 'Manager']

/** Displays every team member with their role; a Manager can change anyone else's role inline. */
export function RosterList() {
  const { data: roster, isLoading, isError } = useRoster()
  const { userId, role } = useAuthStore()
  const updateRole = useUpdateRole()
  const canManageRoles = role === 'Manager'

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie składu…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać składu drużyny.</p>
  }

  return (
    <ul className="flex w-full max-w-sm flex-col divide-y divide-neutral-800 rounded-md border border-neutral-800">
      {roster?.map((member) => (
        <li key={member.id} className="flex items-center justify-between px-4 py-3">
          <span className="font-medium">{member.displayName}</span>

          {canManageRoles && member.id !== userId ? (
            <select
              value={member.role}
              disabled={updateRole.isPending}
              onChange={(event) =>
                updateRole.mutate({ userId: member.id, role: event.target.value as TeamRole })
              }
              className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-sm outline-none focus:border-neutral-500"
            >
              {roles.map((r) => (
                <option key={r} value={r}>
                  {roleLabels[r]}
                </option>
              ))}
            </select>
          ) : (
            <span className="text-sm text-neutral-400">{roleLabels[member.role] ?? member.role}</span>
          )}
        </li>
      ))}
    </ul>
  )
}
