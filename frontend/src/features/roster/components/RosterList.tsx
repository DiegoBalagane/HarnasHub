import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import type { AccessLevel, RosterSlot, TeamMember } from '../../../services/rosterApi'
import {
  accessLevelDescriptions,
  accessLevelLabels,
  accessLevels,
  isCoachDescription,
  isCoachLabel,
  rosterSlotLabels,
  rosterSlots,
} from '../labels'
import { useRoster, useSetIsCoach, useUpdateRole, useUpdateRosterSlot } from '../hooks/useRoster'
import { TeamRoleBadges, TeamRolesEditor } from './TeamRolesEditor'

/** Displays every team member with their roles; a Manager changes access levels, the Coach tag and roster slot, a Coach/Manager the in-game roles. */
export function RosterList() {
  const { data: roster, isLoading, isError } = useRoster()
  const { userId, role } = useAuthStore()
  const updateRole = useUpdateRole()
  const setIsCoach = useSetIsCoach()
  const updateRosterSlot = useUpdateRosterSlot()
  const canManageRoles = role === 'Manager'
  const canManageTeamRoles = useIsCoachOrManager()

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie składu…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać składu drużyny.</p>
  }

  return (
    <div className="flex w-full max-w-xl flex-col gap-2">
      {updateRosterSlot.isError && <p className="text-sm text-red-400">{updateRosterSlot.error.message}</p>}
      {setIsCoach.isError && <p className="text-sm text-red-400">{setIsCoach.error.message}</p>}
      <ul className="flex flex-col divide-y divide-neutral-800 rounded-md border border-neutral-800">
        {roster?.map((member) => (
          <li key={member.id} className="flex items-center justify-between gap-3 px-4 py-3">
            <MemberName member={member} />

            <div className="flex items-center gap-2">
              {canManageTeamRoles ? (
                <TeamRolesEditor member={member} />
              ) : (
                <TeamRoleBadges member={member} />
              )}

              {canManageTeamRoles ? (
                <select
                  value={member.rosterSlot ?? ''}
                  disabled={updateRosterSlot.isPending}
                  aria-label="Status w składzie"
                  onChange={(event) =>
                    updateRosterSlot.mutate({
                      userId: member.id,
                      rosterSlot: event.target.value === '' ? null : (event.target.value as RosterSlot),
                    })
                  }
                  className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-sm outline-none focus:border-neutral-500"
                >
                  <option value="">Nieprzypisany</option>
                  {rosterSlots.map((slot) => (
                    <option key={slot} value={slot}>
                      {rosterSlotLabels[slot]}
                    </option>
                  ))}
                </select>
              ) : (
                member.rosterSlot && (
                  <span className="rounded-full border border-neutral-700 px-2 py-0.5 text-xs text-neutral-300">
                    {rosterSlotLabels[member.rosterSlot]}
                  </span>
                )
              )}

              {canManageRoles ? (
                <label
                  title={member.role === 'Guest' ? 'Najpierw nadaj poziom uprawnień' : isCoachDescription}
                  className="flex items-center gap-1 text-xs text-neutral-300"
                >
                  <input
                    type="checkbox"
                    checked={member.isCoach}
                    disabled={setIsCoach.isPending || member.role === 'Guest'}
                    onChange={(event) =>
                      setIsCoach.mutate({ userId: member.id, isCoach: event.target.checked })
                    }
                  />
                  {isCoachLabel}
                </label>
              ) : (
                member.isCoach && (
                  <span className="rounded-full border border-neutral-700 px-2 py-0.5 text-xs text-neutral-300">
                    {isCoachLabel}
                  </span>
                )
              )}

              {canManageRoles && member.id !== userId ? (
                <select
                  value={member.role}
                  disabled={updateRole.isPending}
                  aria-label="Uprawnienia"
                  title={accessLevelDescriptions[member.role]}
                  onChange={(event) =>
                    updateRole.mutate({ userId: member.id, role: event.target.value as AccessLevel })
                  }
                  className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-sm outline-none focus:border-neutral-500"
                >
                  {accessLevels.map((level) => (
                    <option key={level} value={level}>
                      {accessLevelLabels[level]}
                    </option>
                  ))}
                </select>
              ) : (
                <span
                  title={accessLevelDescriptions[member.role]}
                  className="text-sm text-neutral-400"
                >
                  {accessLevelLabels[member.role] ?? member.role}
                </span>
              )}
            </div>
          </li>
        ))}
      </ul>
    </div>
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
