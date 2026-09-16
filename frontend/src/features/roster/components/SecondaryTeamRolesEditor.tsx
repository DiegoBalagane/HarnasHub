import { useState } from 'react'
import type { TeamMember, TeamRole } from '../../../services/rosterApi'
import { useUpdateSecondaryTeamRoles } from '../hooks/useRoster'
import { teamRoleLabels, teamRoles } from '../labels'

/** Read-only badges for a member's backup roles (e.g. "second AWPer"), shown to anyone who can't edit them. */
export function SecondaryTeamRoleBadges({ member }: { member: TeamMember }) {
  if (member.secondaryTeamRoles.length === 0) {
    return null
  }

  return (
    <>
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

/** Coach/Manager popover: multi-select checkboxes for a member's backup in-game roles, saved on every toggle. */
export function SecondaryTeamRolesEditor({ member }: { member: TeamMember }) {
  const [isOpen, setIsOpen] = useState(false)
  const updateSecondaryTeamRoles = useUpdateSecondaryTeamRoles()
  const selectable = teamRoles.filter((role) => role !== member.teamRole)

  function toggle(role: TeamRole) {
    const isSelected = member.secondaryTeamRoles.includes(role)
    const nextRoles = isSelected
      ? member.secondaryTeamRoles.filter((existing) => existing !== role)
      : [...member.secondaryTeamRoles, role]

    updateSecondaryTeamRoles.mutate({ userId: member.id, teamRoles: nextRoles })
  }

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setIsOpen((open) => !open)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs text-neutral-300 hover:border-neutral-500"
      >
        {member.secondaryTeamRoles.length === 0
          ? '+ rola dodatkowa'
          : member.secondaryTeamRoles.map((role) => teamRoleLabels[role]).join(', ')}
      </button>

      {isOpen && (
        <div className="absolute right-0 z-10 mt-1 flex w-48 flex-col gap-1 rounded-md border border-neutral-800 bg-neutral-950 p-2 shadow-lg">
          {selectable.map((role) => (
            <label key={role} className="flex items-center gap-2 text-xs text-neutral-300">
              <input
                type="checkbox"
                checked={member.secondaryTeamRoles.includes(role)}
                disabled={updateSecondaryTeamRoles.isPending}
                onChange={() => toggle(role)}
              />
              {teamRoleLabels[role]}
            </label>
          ))}
        </div>
      )}
    </div>
  )
}
