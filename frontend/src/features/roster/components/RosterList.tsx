import { useState } from 'react'
import { ConfirmDialog } from '../../../components/ConfirmDialog'
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
import {
  useDeleteMember,
  useRoster,
  useSetIsCoach,
  useUpdateRole,
  useUpdateRosterSlot,
} from '../hooks/useRoster'
import { TeamRoleBadges, TeamRolesEditor } from './TeamRolesEditor'

/** Displays every team member with their roles; a Manager changes access levels, the Coach tag, roster slot and can delete an account. */
export function RosterList() {
  const { data: roster, isLoading, isError } = useRoster()
  const { userId, role } = useAuthStore()
  const updateRole = useUpdateRole()
  const setIsCoach = useSetIsCoach()
  const updateRosterSlot = useUpdateRosterSlot()
  const deleteMember = useDeleteMember()
  const [confirmingUserId, setConfirmingUserId] = useState<string | null>(null)
  const canManageRoles = role === 'Manager'
  const canManageTeamRoles = useIsCoachOrManager()
  const confirmingMember = roster?.find((member) => member.id === confirmingUserId) ?? null

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie składu…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać składu drużyny.</p>
  }

  // Fixed column widths keep every row aligned regardless of nickname/badge length — no per-row layout shifting.
  const gridColumns = 'grid-cols-[minmax(140px,1fr)_minmax(150px,200px)_140px_70px_140px_120px]'

  return (
    <div className="w-full max-w-4xl overflow-x-auto rounded-md border border-neutral-800">
      {updateRosterSlot.isError && (
        <p className="px-4 pt-3 text-sm text-red-400">{updateRosterSlot.error.message}</p>
      )}
      {setIsCoach.isError && <p className="px-4 pt-3 text-sm text-red-400">{setIsCoach.error.message}</p>}
      {deleteMember.isError && <p className="px-4 pt-3 text-sm text-red-400">{deleteMember.error.message}</p>}

      <div className={`grid min-w-[820px] ${gridColumns} gap-x-3 border-b border-neutral-800 px-4 py-2 text-xs text-neutral-500`}>
        <span>Zawodnik</span>
        <span>Role w grze</span>
        <span>Skład</span>
        <span>Trener</span>
        <span>Uprawnienia</span>
        <span className="text-right">Akcje</span>
      </div>

      <ul className="flex min-w-[820px] flex-col divide-y divide-neutral-800">
        {roster?.map((member) => (
          <li key={member.id} className="px-4 py-3">
              <div className={`grid items-center ${gridColumns} gap-x-3 gap-y-1`}>
                <MemberName member={member} />

                <div className="flex min-w-0 flex-wrap items-center gap-1">
                  {canManageTeamRoles ? (
                    <TeamRolesEditor member={member} />
                  ) : (
                    <TeamRoleBadges member={member} />
                  )}
                </div>

                <div className="min-w-0">
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
                      className="w-full rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-sm outline-none focus:border-neutral-500"
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
                </div>

                <div className="min-w-0">
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
                </div>

                <div className="min-w-0">
                  {canManageRoles && member.id !== userId ? (
                    <select
                      value={member.role}
                      disabled={updateRole.isPending}
                      aria-label="Uprawnienia"
                      title={accessLevelDescriptions[member.role]}
                      onChange={(event) =>
                        updateRole.mutate({ userId: member.id, role: event.target.value as AccessLevel })
                      }
                      className="w-full rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-sm outline-none focus:border-neutral-500"
                    >
                      {accessLevels.map((level) => (
                        <option key={level} value={level}>
                          {accessLevelLabels[level]}
                        </option>
                      ))}
                    </select>
                  ) : (
                    <span title={accessLevelDescriptions[member.role]} className="text-sm text-neutral-400">
                      {accessLevelLabels[member.role] ?? member.role}
                    </span>
                  )}
                </div>

                <div className="flex justify-end">
                  {canManageRoles && member.id !== userId && (
                    <button
                      type="button"
                      onClick={() => setConfirmingUserId(member.id)}
                      className="shrink-0 rounded-md border border-neutral-700 px-2 py-1 text-xs text-neutral-400 transition hover:border-red-500 hover:text-red-400"
                    >
                      Usuń z drużyny
                    </button>
                  )}
                </div>
              </div>
          </li>
        ))}
      </ul>

      {confirmingMember && (
        <ConfirmDialog
          title="Usunąć zawodnika?"
          message={
            <>
              Na pewno usunąć <span className="font-medium">{confirmingMember.inGameNickname ?? confirmingMember.displayName}</span>{' '}
              z drużyny? Wyniki, granaty i taktyki które dodał zostają — konto i jego dane osobiste znikają
              bezpowrotnie.
            </>
          }
          confirmLabel="Tak, usuń"
          isConfirming={deleteMember.isPending}
          onConfirm={() =>
            deleteMember.mutate(confirmingMember.id, { onSuccess: () => setConfirmingUserId(null) })
          }
          onCancel={() => setConfirmingUserId(null)}
        />
      )}
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
        <span className="truncate font-medium" title={primaryName}>
          {primaryName}
        </span>
        {showsDiscordName && (
          <span className="truncate text-xs text-neutral-500" title="Nazwa z Discorda">
            {member.displayName}
          </span>
        )}
      </span>
    </span>
  )
}
