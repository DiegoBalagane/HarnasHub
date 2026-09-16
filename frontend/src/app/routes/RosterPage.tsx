import { useAuthStore } from '../../features/auth/stores/useAuthStore'
import { RosterBoard } from '../../features/roster/components/RosterBoard'
import { RosterList } from '../../features/roster/components/RosterList'

export function RosterPage() {
  const role = useAuthStore((state) => state.role)
  const canManageRoster = role === 'Manager' || role === 'Coach'

  return (
    <>
      <h1 className="text-2xl font-semibold">Skład drużyny</h1>
      {canManageRoster && <RosterBoard />}
      <RosterList />
    </>
  )
}
