import { useState } from 'react'
import type { TeamMember, TeamRole } from '../../../services/rosterApi'
import { useUpdateSecondaryTeamRoles, useUpdateTeamRole } from '../hooks/useRoster'
import { teamRoleLabels, teamRoles } from '../labels'

/** Read-only badges for a member's roles: the primary one first, then any backups (e.g. "second AWPer"). */
export function TeamRoleBadges({ member }: { member: TeamMember }) {
  if (member.teamRole === null && member.secondaryTeamRoles.length === 0) {
    return null
  }

  return (
    <>
      {member.teamRole && (
        <span
          title="Rola główna"
          className="rounded-full border border-neutral-700 px-2 py-0.5 text-[11px] text-neutral-300"
        >
          {teamRoleLabels[member.teamRole]}
        </span>
      )}
      {member.secondaryTeamRoles.map((role) => (
        <span
          key={role}
          title="Rola dodatkowa"
          className="rounded-full border border-neutral-800 px-2 py-0.5 text-[11px] text-neutral-500"
        >
          +{teamRoleLabels[role]}
        </span>
      ))}
    </>
  )
}

/** Coach/Manager popover: one control for a member's whole set of in-game roles — check any that apply, star marks which is primary. */
export function TeamRolesEditor({ member }: { member: TeamMember }) {
  const [isOpen, setIsOpen] = useState(false)
  const updateTeamRole = useUpdateTeamRole()
  const updateSecondaryTeamRoles = useUpdateSecondaryTeamRoles()
  const isPending = updateTeamRole.isPending || updateSecondaryTeamRoles.isPending

  function setPrimary(role: TeamRole) {
    if (role === member.teamRole) return

    updateTeamRole.mutate({ userId: member.id, teamRole: role })

    const nextSecondary = member.secondaryTeamRoles.filter((existing) => existing !== role)
    // The role that was primary keeps applying to this member, just demoted to a backup role.
    const demoted = member.teamRole !== null ? [...nextSecondary, member.teamRole] : nextSecondary

    updateSecondaryTeamRoles.mutate({ userId: member.id, teamRoles: demoted })
  }

  function toggleActive(role: TeamRole, isChecked: boolean) {
    if (!isChecked) {
      if (role === member.teamRole) {
        updateTeamRole.mutate({ userId: member.id, teamRole: null })
      } else {
        updateSecondaryTeamRoles.mutate({
          userId: member.id,
          teamRoles: member.secondaryTeamRoles.filter((existing) => existing !== role),
        })
      }
      return
    }

    if (member.teamRole === null) {
      setPrimary(role)
    } else {
      updateSecondaryTeamRoles.mutate({ userId: member.id, teamRoles: [...member.secondaryTeamRoles, role] })
    }
  }

  const summary =
    member.teamRole === null && member.secondaryTeamRoles.length === 0 ? (
      '+ role'
    ) : (
      <>
        {member.teamRole && teamRoleLabels[member.teamRole]}
        {member.secondaryTeamRoles.length > 0 &&
          `${member.teamRole ? ', ' : ''}${member.secondaryTeamRoles.map((role) => teamRoleLabels[role]).join(', ')}`}
      </>
    )

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setIsOpen((open) => !open)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs text-neutral-300 hover:border-neutral-500"
      >
        {summary}
      </button>

      {isOpen && (
        <div className="absolute right-0 z-10 mt-1 flex w-56 flex-col gap-1 rounded-md border border-neutral-800 bg-neutral-950 p-2 shadow-lg">
          <p className="px-1 pb-1 text-[10px] text-neutral-500">Zaznacz role, gwiazdką ustaw główną</p>
          {teamRoles.map((role) => {
            const isActive = role === member.teamRole || member.secondaryTeamRoles.includes(role)
            const isPrimary = role === member.teamRole

            return (
              <div key={role} className="flex items-center gap-2 text-xs text-neutral-300">
                <input
                  type="checkbox"
                  checked={isActive}
                  disabled={isPending}
                  onChange={(event) => toggleActive(role, event.target.checked)}
                />
                <span className="flex-1">{teamRoleLabels[role]}</span>
                <button
                  type="button"
                  title={isPrimary ? 'Rola główna' : 'Ustaw jako główną'}
                  disabled={isPending}
                  onClick={() => setPrimary(role)}
                  className={`text-sm transition disabled:opacity-40 ${
                    isPrimary ? 'text-amber-400' : 'text-neutral-700 hover:text-neutral-400'
                  }`}
                >
                  ★
                </button>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
