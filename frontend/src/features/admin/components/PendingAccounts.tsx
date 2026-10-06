import type { AccessLevel, TeamMember } from '../../../services/rosterApi'
import { Button } from '../../../components/ui/Button'
import { useRoster, useUpdateRole } from '../../roster/hooks/useRoster'
import { accessLevelLabels } from '../../roster/labels'

/** "Oczekujące konta": Guests awaiting access, with one-click grant of the Player or Manager level. */
export function PendingAccounts() {
  const { data: roster } = useRoster()
  const pending = (roster ?? []).filter((member) => member.role === 'Guest')

  if (pending.length === 0) {
    return null
  }

  return (
    <section className="flex flex-col gap-2 rounded-md border border-primary-500/40 bg-primary-500/5 p-4">
      <h2 className="text-sm font-medium text-neutral-200">Oczekujące konta ({pending.length})</h2>
      <ul className="flex flex-col gap-2">
        {pending.map((member) => (
          <PendingAccountRow key={member.id} member={member} />
        ))}
      </ul>
    </section>
  )
}

const grantableLevels: AccessLevel[] = ['Player', 'Manager']

function PendingAccountRow({ member }: { member: TeamMember }) {
  const updateRole = useUpdateRole()

  return (
    <li className="flex flex-wrap items-center gap-3">
      {member.avatarUrl && <img src={member.avatarUrl} alt="" className="h-6 w-6 rounded-full" />}
      <span className="min-w-0 flex-1 truncate text-sm">{member.displayName}</span>
      {grantableLevels.map((level) => (
        <Button
          key={level}
          size="sm"
          variant={level === 'Player' ? 'primary' : 'secondary'}
          disabled={updateRole.isPending}
          onClick={() => updateRole.mutate({ userId: member.id, role: level })}
        >
          Nadaj: {accessLevelLabels[level]}
        </Button>
      ))}
      {updateRole.isError && <span className="w-full text-xs text-danger-400">{updateRole.error.message}</span>}
    </li>
  )
}
