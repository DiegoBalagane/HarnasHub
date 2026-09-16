import { useAuthStore } from '../../auth/stores/useAuthStore'
import type { TeamMember, TeamRole, UserRole } from '../../../services/rosterApi'
import { roleLabels, teamRoleLabels, teamRoles, userRoles } from '../labels'
import { useRoster, useUpdateRole, useUpdateTeamRole } from '../hooks/useRoster'

/** Displays every team member with their roles; a Manager changes access levels, a Coach/Manager the in-game roles. */
export function RosterList() {
  const { data: roster, isLoading, isError } = useRoster()
  const { userId, role } = useAuthStore()
  const updateRole = useUpdateRole()
  const updateTeamRole = useUpdateTeamRole()
  const canManageRoles = role === 'Manager'
  const canManageTeamRoles = role === 'Manager' || role === 'Coach'

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie składu…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać składu drużyny.</p>
  }

  return (
    <ul className="flex w-full max-w-xl flex-col divide-y divide-neutral-800 rounded-md border border-neutral-800">
      {roster?.map((member) => (
        <li key={member.id} className="flex items-center justify-between gap-3 px-4 py-3">
          <MemberName member={member} />

          <div className="flex items-center gap-2">
            {canManageTeamRoles ? (
              <select
                value={member.teamRole ?? ''}
                disabled={updateTeamRole.isPending}
                aria-label="Rola w drużynie"
                onChange={(event) =>
                  updateTeamRole.mutate({
                    userId: member.id,
                    teamRole: event.target.value === '' ? null : (event.target.value as TeamRole),
                  })
                }
                className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-sm outline-none focus:border-neutral-500"
              >
                <option value="">Brak roli</option>
                {teamRoles.map((teamRole) => (
                  <option key={teamRole} value={teamRole}>
                    {teamRoleLabels[teamRole]}
                  </option>
                ))}
              </select>
            ) : (
              member.teamRole && (
                <span className="rounded-full border border-neutral-700 px-2 py-0.5 text-xs text-neutral-300">
                  {teamRoleLabels[member.teamRole]}
                </span>
              )
            )}

            {canManageRoles && member.id !== userId ? (
              <select
                value={member.role}
                disabled={updateRole.isPending}
                aria-label="Uprawnienia"
                onChange={(event) =>
                  updateRole.mutate({ userId: member.id, role: event.target.value as UserRole })
                }
                className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-sm outline-none focus:border-neutral-500"
              >
                {userRoles.map((r) => (
                  <option key={r} value={r}>
                    {roleLabels[r]}
                  </option>
                ))}
              </select>
            ) : (
              <span className="text-sm text-neutral-400">{roleLabels[member.role] ?? member.role}</span>
            )}
          </div>
        </li>
      ))}
    </ul>
  )
}

/** Shows the in-game nickname as the primary name, keeping the Discord name as a secondary hint. */
function MemberName({ member }: { member: TeamMember }) {
  const primaryName = member.inGameNickname ?? member.displayName
  const showsDiscordName = member.inGameNickname !== null && member.inGameNickname !== member.displayName

  return (
    <span className="flex min-w-0 items-center gap-2">
      {member.avatarUrl && <img src={member.avatarUrl} alt="" className="h-6 w-6 shrink-0 rounded-full" />}
      <span className="flex min-w-0 flex-col">
        <span className="truncate font-medium">{primaryName}</span>
        {showsDiscordName && (
          <span className="truncate text-xs text-neutral-500" title="Nazwa z Discorda">
            {member.displayName}
          </span>
        )}
      </span>
    </span>
  )
}
