import type { TeamMember } from '../../../services/rosterApi'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { isCoachLabel, rosterSlotLabels } from '../labels'
import { useRoster } from '../hooks/useRoster'
import { TeamRoleBadges, TeamRolesEditor } from './TeamRolesEditor'

/** Team view of every member: nickname, in-game roles (editable by Coach/Manager), roster slot and Coach badge. Account administration lives in the admin panel. */
export function RosterList() {
  const { data: roster, isLoading, isError } = useRoster()
  const canManageTeamRoles = useIsCoachOrManager()

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie składu…</p>
  }

  if (isError) {
    return <p className="text-danger-400">Nie udało się pobrać składu drużyny.</p>
  }

  // Fixed column widths keep every row aligned regardless of nickname/badge length — no per-row layout shifting.
  const gridColumns = 'grid-cols-[minmax(140px,1fr)_minmax(150px,260px)_140px_90px]'

  return (
    <div className="w-full overflow-x-auto rounded-md border border-neutral-800">
      <div className={`grid min-w-[620px] ${gridColumns} gap-x-3 border-b border-neutral-800 px-4 py-2 text-xs text-neutral-500`}>
        <span>Zawodnik</span>
        <span>Role w grze</span>
        <span>Skład</span>
        <span>Trener</span>
      </div>

      <ul className="flex min-w-[620px] flex-col divide-y divide-neutral-800">
        {roster
          ?.filter((member) => member.role !== 'Guest')
          .map((member) => (
            <li key={member.id} className="px-4 py-3">
              <div className={`grid items-center ${gridColumns} gap-x-3 gap-y-1`}>
                <MemberName member={member} />

                <div className="flex min-w-0 flex-wrap items-center gap-1">
                  {canManageTeamRoles ? <TeamRolesEditor member={member} /> : <TeamRoleBadges member={member} />}
                </div>

                <div className="min-w-0">
                  {member.rosterSlot && <Badge>{rosterSlotLabels[member.rosterSlot]}</Badge>}
                </div>

                <div className="min-w-0">{member.isCoach && <Badge>{isCoachLabel}</Badge>}</div>
              </div>
            </li>
          ))}
      </ul>
    </div>
  )
}

function Badge({ children }: { children: string }) {
  return <span className="rounded-full border border-neutral-700 px-2 py-0.5 text-xs text-neutral-300">{children}</span>
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
